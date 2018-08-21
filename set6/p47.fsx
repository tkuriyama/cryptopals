#load "utils.fs"

open Utils
open System
open System.Numerics

let rnd = new Random()
let ((e, n), (d, _)) = Utils.genRSAKeys rnd 128
let k = n.ToByteArray() |> Array.length
    
(* Padding and Oracle *)

let parityOracle d n c : bool =
    let m = Utils.decryptRSA d n c
    let mArr = m.ToByteArray() |> Array.rev
    let pad = repeatArr 0uy (k - (Array.length mArr))
    let mArr' = Array.append pad mArr
    mArr'.[0..1] = [|0uy; 2uy|]

let genNonZeroRandArr (size: int) : byte [] =
    let rnd = Random()
    [|for _ in 1..size do yield rnd.Next 254 |> (+) 1 |> byte|]

let padPKCS15 (m: string) : byte [] =
    let mArr = Utils.strToBytes m
    let PS = genNonZeroRandArr (k - 3 - (Array.length mArr))
    Array.concat [| [| 0uy; 2uy |]; PS; [| 0uy |]; mArr |] |> Array.rev

(* Padding and Oracle Test *)

let padded = padPKCS15 "kick it, CC"
let c = padded |> BigInteger |> Utils.encryptRSA e n
let m  = Utils.decryptRSA d n c
let mStr = m.ToByteArray() |> Array.rev |> Utils.bytesToStr
let testParityOracle = parityOracle d n c

(* Parity Oracle Decryption *)

let B = BigInteger.Pow (BigInteger 2, (8 * (k-2)))
let zero, one, two, three = (BigInteger 0, BigInteger 1,
                             BigInteger 2, BigInteger 3)
let M = [| (two * B, three * B - one) |]

let checkOracle s =
    (c * BigInteger.ModPow (s, e, n)) % n |> parityOracle d n

let initSearch =
    let rec search s =
        match checkOracle s with
        | true -> s
        | _    -> search (s + one)
    search (n / (three * B))

let genInterval (M: (BigInteger * BigInteger) []) (s: BigInteger) =
    let a, b = M.[0]
    let r = (two * (b * s - two * B)) / n
    
    let rec search r =        
        let rec innerSearch sRange =
            match sRange with
            | x::xs -> if checkOracle x then x else innerSearch xs
            | []    -> zero
        let sLow = (two * B + r * n) / b
        let sHigh = (three * B + r * n) / a
        let sRange = if sLow > sHigh then failwith "out of range"
                     else [sLow .. sHigh]
        let s = innerSearch sRange
        match s = zero with
        | true -> search (r + one)
        | _    -> s
    search r, a, b

let updateInterval (M: (BigInteger * BigInteger) []) (s: BigInteger) =
    let s', a, b = genInterval M s

    let minR = (a * s' - three * B + one) / n
    let maxR = (b * s' - two * B) / n 
    let r = if minR > maxR then failwith "r range incorrect" else minR
    
    let a' = max a ((two * B  + r * n + s' - one) / s')
    let b' = min b ((three * B - one +  r * n) / s')
    let M' = if a' > b' then failwith "invalid range" else [| (a', b') |]
    M', s'

let rec solve (M: (BigInteger * BigInteger) []) (s: BigInteger) =
    let rec loop M s ctr =
        match ctr > 100 with
        | true -> zero
        | _    -> let M', s' = updateInterval M s
                  let a, b = M'.[0]
                  printfn "> iteration %d" ctr
                  if a = b then a else loop M' s' (ctr + 1)
    loop M s 1

let testGenInterval = genInterval M initSearch
let decrypted = solve M initSearch
