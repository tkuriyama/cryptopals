#load "utils.fs"

open Utils
open System
open System.Numerics

let primes = Utils.primes

let r = new Random()
let p (r: Random) = Seq.take (1000 + (r.Next(1000))) primes |> Seq.last
let q (r: Random) = Seq.take (2000 + (r.Next(1000))) primes |> Seq.last
let n1 = (p r) * (q r)
let n2 = (p r) * (q r)
let n3 = (p r) * (q r)

let e = BigInteger 3
let msg = BigInteger 9999

let encrypt n e msg = BigInteger.ModPow (msg, e, n)

let c1 = encrypt n1 e msg
let c2 = encrypt n2 e msg
let c3 = encrypt n3 e msg

let solve = 
    let ms1, ms2, ms3 = n2 * n3, n1 * n3, n1 * n2
    let r1 = c1 * ms1 * (Utils.modInvBig ms1 n1).Value
    let r2 = c2 * ms2 * (Utils.modInvBig ms2 n2).Value
    let r3 = c3 * ms3 * (Utils.modInvBig ms3 n3).Value
    BigInteger.ModPow (r1 + r2 + r3, (BigInteger 1), n1 * n2 * n3)

let test = msg * msg * msg |> (=) solve
