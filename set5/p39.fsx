#load "utils.fs"

open Utils
open System
open System.Numerics

let r = new Random()
let ((e, n), (d, _)) = Utils.genRSAKeys r

let c = BigInteger.ModPow (BigInteger 42, e, n)
let m = BigInteger.ModPow (c, d, n)

let msg = "test" |> Utils.strToBytes |> BigInteger
let code = BigInteger.ModPow (msg, e, n)
let plain =
    let b = BigInteger.ModPow (code, d, n)
    b.ToByteArray() |> Utils.bytesToStr

