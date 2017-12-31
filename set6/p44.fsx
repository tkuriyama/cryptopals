#load "utils.fs"

open Utils
open System
open System.Numerics

let y = "2d026f4bf30195ede3a088da85e398ef869611d0f68f0713d51c9c1a3a26c95105d915e2d8cdf26d056b86b8a7b85519b1c23cc3ecdc6062650462e3063bd179c2a6581519f674a61f1d89a1fff27171ebc1b93d4dc57bceb7ae2430f98a6a4d83d8279ee65d71c1203d2c96d65ebbf7cce9d32971c3de5084cce04a2e147821" |> Utils.hexToBigInt

let rand = new Random()
let _, _, q, p, g = Utils.genDSAKeys rand

let inputs =
    Utils.readLines "p44_data.txt"
    |> Seq.map (fun (l: string) -> l.Split [|':'|])
    |> Seq.map (fun (a: string []) -> a.[1])
    |> Seq.map (fun (s: string) -> s.[1..])
    |> Seq.toList
    |> List.chunkBySize 4
