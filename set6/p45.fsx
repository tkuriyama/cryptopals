#load "utils.fs"
#load "sha1.fs"

open Utils
open Sha1
open System
open System.Numerics

let q = "f4f47f05794b256174bba6e9b396a7707e563c5b" |> hexToBigInt
let p = "800000000000000089e1855218a0e7dac38136ffafa72eda7859f2171e25e65eac698c1702578b07dc2a1076da241c76c62d374d8389ea5aeffd3226a0530cc565f3bf6b50929139ebeac04f48c3c84afb796d61e5a4f9a8fda812ab59494232c7d2b4deb50aa18ee9e132bfa85ac4374d7f9091abc3d015efc871a584471bb1" |> hexToBigInt
let g = "5958c9d3898b224b12672c0b98e06c60df923cb8bc999d119458fef538b8fa4046c8db53039db620c094c9fa077ef389b5322a559946a71903f990f1f7e0e025e2d7f7cf494aff1a0470f5b64c36b625a097f1651fe775323556fe00b3608c887892878480e99041be601a62166ca6894bdd41a7054ec89f756ba9fc95302291" |> hexToBigInt

let rand = new Random()
let genDSAKeys (rand: Random) q p g =
    let x = 1 + rand.Next(1000000000) |> BigInteger
    let pub = BigInteger.ModPow (g, x, p)
    (x, pub, q, p, g)

let d1 = Utils.strToBytes "Hello, World" |> Sha1.sha1 |> hexToBigInt 
let d2 = Utils.strToBytes "Goodbye, world" |> Sha1.sha1 |> hexToBigInt

(* g = 0 *)

let x1, y1, _, _, g1 = genDSAKeys rand q p (BigInteger 0)
let r1, s1, k1 = Utils.signDSARandK x1 q p g1 d2 rand
let verifyD1 = Utils.verifyDSA q p y1 g1 d1 r1 s1
let verifyD2 = Utils.verifyDSA q p y1 g1 d2 r1 s1

(* g = (p+1) *)

let _, y2, _, _, g2 = genDSAKeys rand q p (p + (BigInteger 1))
let z = (BigInteger 3)
let z' =
    match (Utils.modInvBig z q) with
    | Some z' -> z'
    | None    -> (BigInteger 0)
let r2 = (BigInteger.ModPow (y2, z, p)) % q          
let s2 = (z' * r2) % q
let verifyD1' = Utils.verifyDSA q p y2 g2 d1 r2 s2
let verifyD2' = Utils.verifyDSA q p y2 g2 d2 r2 s2
