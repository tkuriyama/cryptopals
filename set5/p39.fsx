#load "utils.fs"

open Utils
open System
open System.Numerics

let primes = Utils.readLines "primes.csv" |> Seq.map int

let r = new Random()
let p = Seq.take (r.Next(1000)) primes |> Seq.last;;
let q = Seq.take (r.Next(1000)) primes |> Seq.last;;
let n = p * q

let et = (p - 1) * (q - 1)
let e = 3
let d = Utils.modInv e et

let c = BigInteger.ModPow (BigInteger 42, BigInteger e, BigInteger n)
let m = BigInteger.ModPow (c, BigInteger (d.Value), BigInteger n)

