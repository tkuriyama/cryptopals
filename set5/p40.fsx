#load "utils.fs"

open Utils
open System
open System.Numerics


let primes = Utils.readLines "primes_10K.txt" |> Seq.map int |> Seq.map BigInteger

let r = new Random()
let p (r: Random) = Seq.take (8000 + (r.Next(1000))) primes |> Seq.last
let q (r: Random) = Seq.take (9000 + (r.Next(1000))) primes |> Seq.last
let n1 = (p r) * (q r)
let n2 = (p r) * (q r)
let n3 = (p r) * (q r)

let e = BigInteger 3
let msg = BigInteger 9999

let encrypt n e msg = BigInteger.ModPow (msg, e, n)

let c1 = encrypt n1 e msg
let c2 = encrypt n2 e msg
let c3 = encrypt n3 e msg

let solve c1 n1 c2 n2 c3 n3 : BigInteger =
    let ms1, ms2, ms3 = n2 * n3, n1 * n3, n1 * n2
    (c1 * ms1 * (Utils.modInvBig ms1 n1).Value)
    |> (+) (c2 * ms2 * (Utils.modInvBig ms2 n2).Value)
    |> (+) (c3 + ms3 * (Utils.modInvBig ms3 n3).Value)

