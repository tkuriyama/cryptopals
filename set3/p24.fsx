#load "utils.fs"

open Utils
open System

let testCode = "Yellow submarine, testing testing" 

let applyCTR (seed: uint32) (code: byte []) : byte [] =
    Utils.xorArr (MTSequence seed |> Seq.map byte) code

let encryptTest = Utils.strToBytes testCode |> applyCTR 0u
let decryptTest = applyCTR 0u encryptTest |> Utils.bytesToStr

let rnd = Random()
let seed = let max = pown 2 16 in rnd.Next(max) |> uint32
let input = 
    "AAAAAAAAAAAAAA"
    |> Utils.strToBytes
    |> Array.append (Utils.randKey (5 + rnd.Next(5)))
let code = applyCTR seed input

let tryFind 

let findSeed code input : int =
    let rec f code i =
        match i with
        | 65536 -> 65536
        | _     -> let found = tryFind code i
                   if found = true then i else f code (i+1)
    f code 1
