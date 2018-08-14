#load "utils.fs"

open Utils
open System
open System.Numerics

let rnd = new Random()
let ((e, n), (d, _)) = Utils.genRSAKeys rnd 128

let parityOracle d n c : bool =
    let m' = Utils.decryptRSA d n c
    let mArr = m'.ToByteArray() |> Array.rev
    let k = n.ToByteArray() |> Array.length
    let pad = repeatArr 0uy (k - (Array.length mArr))
    let mArr' = Array.append pad mArr
    mArr'.[0..1] = [|0uy; 2uy|]

let genNonZeroRandArr (size: int) : byte [] =
    let rnd = Random()
    [|for _ in 1..size do yield rnd.Next 255 |> (+) 1 |> byte|]

let padPKCS15 (m: string) =
    let mArr = Utils.strToBytes m
    let k = n.ToByteArray() |> Array.length
    let PS = genNonZeroRandArr (k - 3 - (Array.length mArr))
    Array.concat [| [| 0uy; 2uy |]; PS; [| 0uy |]; mArr |]
    
let c = padPKCS15 "kick it, CC" |> Array.rev |> BigInteger |> Utils.encryptRSA e n
