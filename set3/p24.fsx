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
let target = "AAAAAAAAAAAAAA" |> Utils.strToBytes
let code =
    target
    |> Array.append (Utils.randKey (5 + rnd.Next(5)))
    |> applyCTR seed

let tryFind code i target : bool =
    let guess = applyCTR (uint32 i) code
    let s = (Array.length guess) - (Array.length target)
    if guess.[s..] = target then true else false

let findSeed code target : int =
    let rec f code i =
        match i with
        | 65536 -> 65536
        | _     -> let found = tryFind code i target
                   if found = true then i else f code (i+1)
    f code 1
