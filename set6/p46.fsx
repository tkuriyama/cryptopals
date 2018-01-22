#load "utils.fs"

open Utils
open System
open System.Numerics

let rand = new Random()
let ((e, n), (d, _)) = Utils.genRSAKeys rand 1024

let parityOracle d n c : bool =
    let m' = Utils.decryptRSA d n c
    m'.IsEven

let m = Convert.FromBase64String "VGhhdCdzIHdoeSBJIGZvdW5kIHlvdSBkb24ndCBwbGF5IGFyb3VuZCB3aXRoIHRoZSBGdW5reSBDb2xkIE1lZGluYQ=="
let c = Utils.encryptRSA e n (BigInteger m)

let numToStr (n: BigInteger) : string = n.ToByteArray() |> Utils.bytesToStr

let decrypt c e n oracle =
    let two = BigInteger.ModPow (BigInteger 2, e, n)
    let iters = BigInteger.Log n / BigInteger.Log (BigInteger 2)
                |> int |> (+) 1
    printfn "%d" iters
    let rec loop c iter low high =
        match iters - iter with
        | 0 -> high
        | _ -> let c' = (c * two) % n
               let d = (high - low) / (BigInteger 2)
               printfn "%s" (numToStr high)
               match oracle c' with
               | true -> loop c' (iter + 1) low (high - d)
               | _    -> loop c' (iter + 1) (low + d) high
    loop c 0 (BigInteger 0) n

let guessNum = decrypt c e n (parityOracle d n)
let guess = numToStr guessNum
