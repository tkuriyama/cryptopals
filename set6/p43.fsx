#load "utils.fs"
#load "sha1.fs"

open Utils
open Sha1
open System
open System.Numerics

let rand = new Random()
let x, pub, q, p, g = Utils.genDSAKeys rand

let message = "hello world" |> Utils.strToBytes
let digest = Sha1.sha1 message |> Utils.hexToBigInt
let r, s, k = Utils.signDSA x q p g rand digest
let verify = Utils.verifyDSA q p g pub digest r s

let findPrivate q r s digest k : BigInteger =
    match (Utils.modInvBig r q) with 
    | Some r' -> (r' * (s * k - digest)) % q
    | _       -> BigInteger 0
let testFindPrivate = findPrivate q r s digest k
