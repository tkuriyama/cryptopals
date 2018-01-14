#load "utils.fs"

open Utils
open System
open System.Numerics

let r = new Random()
let ((e, n), (d, _)) = Utils.genRSAKeys r 1024

let parityOracle d n c : bool =
    let m' = Utils.decryptRSA d n c
    m'.IsEven

let m = Convert.FromBase64String "VGhhdCdzIHdoeSBJIGZvdW5kIHlvdSBkb24ndCBwbGF5IGFyb3VuZCB3aXRoIHRoZSBGdW5reSBDb2xkIE1lZGluYQ=="
let c = Utils.encryptRSA e n (BigInteger m)

let decrypt c e n =
    let two = BigInteger.ModPow (BigInteger 2, e, n)
    let oracle = parityOracle d n
    let rec loop c iter (low: BigInteger) (high: BigInteger) =
        match iter with
        | 1024 -> high
        | _    -> let c' = (c * two) % n
                  let d = (high - low) / (BigInteger 2)
                  match oracle c' with
                  | true -> loop c' (iter + 1) low (high - d)
                  | _    -> loop c' (iter + 1) (low + d) high
    loop c 0 (BigInteger 0) n

let guessNum = decrypt c e n
let guess = guessNum.ToByteArray() |> Utils.bytesToStr
