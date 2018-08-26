#load "utils.fs"

open Utils
open System
open System.Numerics

let rnd = new Random()
let ((e, n), (d, _)) = Utils.genRSAKeys rnd 384
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

let rec search s =
    match checkOracle s with
    | true -> s
    | _    -> search (s + one)

let initSearch = search ((n + three * B - one) / (three * B))

let multiIntervalSearch (M: (BigInteger * BigInteger) []) (s: BigInteger) =
    search s    

let singleIntervalSearch (M: (BigInteger * BigInteger) []) (s: BigInteger) =
    let a, b = M.[0]
    let r = (two * (b * s - two * B) + n - one) / n
    
    let rec search r =        
        let rec innerSearch sRange =
            match sRange with
            | x::xs -> if checkOracle x then x else innerSearch xs
            | []    -> zero
        let sLow = (two * B + r * n + b - one) / b
        let sHigh = (three * B + r * n + a - one) / a
        let sRange = if sLow > sHigh then failwith "out of range"
                     else [sLow .. sHigh]
        let s = innerSearch sRange
        if s = zero then search (r + one) else s
    search r

let nextSearch (M: (BigInteger * BigInteger) []) (s: BigInteger) =
    match M.Length = 1 with
    | true -> singleIntervalSearch M s
    | _    -> multiIntervalSearch M s

let updateInterval (M: (BigInteger * BigInteger) []) (s: BigInteger) =
    let rec inner a b rRange arr =
        match rRange with
        | r::rs -> let a' = max a ((two * B  + r * n + s - one) / s)
                   let b' = min b ((three * B - one +  r * n) / s)
                   let i = if a' > b' then failwith "invalid" else [|(a', b')|]
                   inner a b rs (Array.append arr i)
        | []    -> arr

    let newIntervals =
        [| for a, b in M do
           yield let minR = (a * s - three * B + one + n - one) / n in
                 let maxR = (b * s - two * B) / n in
                 let rRange = if minR > maxR then [minR] else [minR .. maxR] in
                 inner a b rRange [||] |]
    Array.concat newIntervals

let rec solve (M: (BigInteger * BigInteger) []) (s: BigInteger) =
    let rec loop (M: (BigInteger * BigInteger) []) s ctr =
        match ctr > 1000 with
        | true -> zero
        | _    -> printfn "> iteration %d" ctr
                  let M' = updateInterval M s
                  match M'.Length = 1 with
                  | true  -> let a, b = M'.[0]
                             if a = b then a
                             else loop M' (nextSearch M' s) (ctr + 1)
                  | _     -> loop M' (nextSearch M' s) (ctr + 1)
    loop M s 1

let testInterval = updateInterval M initSearch
let decrypted = let m = solve M initSearch
                m.ToByteArray() |> Array.rev |> Utils.bytesToStr
