#load "utils.fs"
#load "sha1.fs"

open Utils
open Sha1
open System
open System.Numerics

let rand = new Random()
let x, y, q, p, g = Utils.genDSAKeys rand

(* Test Sign and Verification *)

let message = "hello world" |> Utils.strToBytes
let digest = Sha1.sha1 message |> Utils.hexToBigInt
let k = 1 + rand.Next(1000000000) |> BigInteger
let r, s = Utils.signDSA x q p g k digest
let verify = Utils.verifyDSA q p g y digest r s

(* Find Private Key given k *)

let findPrivate q r s digest k : BigInteger =
    match (Utils.modInvBig r q) with 
    | Some r' -> (r' * (s * k - digest)) % q
    | _       -> BigInteger 0
let testFindPrivate = findPrivate q r s digest k |> (=) x

(* Main Problem *)

let y2 = "84ad4719d044495496a3201c8ff484feb45b962e7302e56a392aee4abab3e4bdebf2955b4736012f21a08084056b19bcd7fee56048e004e44984e2f411788efdc837a0d2e5abb7b555039fd243ac01f0fb2ed1dec568280ce678e931868d23eb095fde9d3779191b8c0299d6e07bbb283e6633451e535c45513b2d33c99ea17" |> Utils.hexToBigInt

let message2 = "For those that envy a MC it can be hazardous to your health
So be friendly, a matter of life and death, just like a etch-a-sketch\n"
let digest2 = message2 |> Utils.strToBytes |> Sha1.sha1 |> Utils.hexToBigInt
let hash2 = "d2d0714f014a9784047eaeccf956520045c45265"
let testHash2 = (message2 |> Utils.strToBytes |> Sha1.sha1) = hash2

let r2 = BigInteger.Parse "548099063082341131477253921760299949438196259240"
let s2 = BigInteger.Parse "857042759984254168557880549501802188789837994940"

let findPrivateBrute q r s digest y : int * BigInteger =
    let rec loop (k: int) e =
        match (e - k) with
        | 0 -> (0, BigInteger 0)
        | _ -> let guess = findPrivate q r s digest (BigInteger k)
               if guess < (BigInteger 0) then loop (k+1) e 
               elif BigInteger.ModPow (g, guess, p) = y then (k, guess)
               else loop (k+1) e
    loop 1 (pown 2 16)
let k2, x2 = findPrivateBrute q r2 s2 digest2 y2
let r2', s2' = Utils.signDSA x2 q p g (BigInteger k2) digest2
let testFindPrivateBrute = (r2 = r2') && (s2 = s2')
