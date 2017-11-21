#load "utils.fs"

open Utils
open System
open System.Numerics

let primes = Utils.readLines "primes_10K.txt" |> Seq.map int |> Seq.map BigInteger

let r = new Random()
let p = Seq.take (5000 + (r.Next(5000))) primes |> Seq.last 
let q = Seq.take (5000 + (r.Next(5000))) primes |> Seq.last 
let n = p * q

let et = (p - (BigInteger 1)) * (q - (BigInteger 1))
let e = BigInteger 3
let d = Utils.modInvBig e et

(* Test Int *)

let c = BigInteger.ModPow (BigInteger 42, e, n)
let m = BigInteger.ModPow (c, d.Value, n)

(* Test String *)

let msg = "Test" |> Utils.strToBytes |> BigInteger
let code = BigInteger.ModPow (msg,  e, n)
let plain =
    let b = BigInteger.ModPow (code, d.Value, n)
    b.ToByteArray() |> Utils.bytesToStr

