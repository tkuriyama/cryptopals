#load "utils.fs"

open Utils
open System
open System.Numerics

let r = new Random()
let ((e, n), (d, _)) = Utils.genRSAKeys r

let msg = BigInteger 99990033
let c = BigInteger.ModPow (msg, e, n)

let s = BigInteger 7
let c' = (((BigInteger.ModPow (s, e, n)) * c) % n)
let p' = BigInteger.ModPow (c', d, n)
let p = (p' * (Utils.modInvBig s n).Value) % n

let test = msg = p
