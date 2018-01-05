#load "utils.fs"

open Utils
open System
open System.Numerics

let rand = new Random()
let _, _, q, p, g = Utils.genDSAKeys rand

let y = "2d026f4bf30195ede3a088da85e398ef869611d0f68f0713d51c9c1a3a26c95105d915e2d8cdf26d056b86b8a7b85519b1c23cc3ecdc6062650462e3063bd179c2a6581519f674a61f1d89a1fff27171ebc1b93d4dc57bceb7ae2430f98a6a4d83d8279ee65d71c1203d2c96d65ebbf7cce9d32971c3de5084cce04a2e147821" |> Utils.hexToBigInt
let r = BigInteger.Parse "548099063082341131477253921760299949438196259240"
let s = BigInteger.Parse "857042759984254168557880549501802188789837994940"

let inputs =
    Utils.readLines "p44_data.txt"
    |> Seq.map (fun (l: string) -> l.Split [|':'|])
    |> Seq.map (fun (a: string []) -> a.[1])
    |> Seq.map (fun (s: string) -> s.[1..])
    |> Seq.toList
    |> List.chunkBySize 4

let findPrivate q r s digest k : BigInteger =
    match (Utils.modInvBig r q) with 
    | Some r' -> (r' * (s * k - digest)) % q
    | _       -> BigInteger 0

let findPrivateBrute q r s digest y : int * BigInteger =
    let rec loop (k: int) e =
        match (e - k) with
        | 0 -> (0, BigInteger 0)
        | _ -> let guess = findPrivate q r s digest (BigInteger k)
               if guess < (BigInteger 0) then loop (k+1) e 
               elif BigInteger.ModPow (g, guess, p) = y then (k, guess)
               else loop (k+1) e
    loop 1 (pown 2 16)
