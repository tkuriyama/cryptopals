#load "utils.fs"

open Utils
open System
open System.Numerics

let rand = new Random()
let _, _, q, p, g = Utils.genDSAKeys rand

let y = "2d026f4bf30195ede3a088da85e398ef869611d0f68f0713d51c9c1a3a26c95105d915e2d8cdf26d056b86b8a7b85519b1c23cc3ecdc6062650462e3063bd179c2a6581519f674a61f1d89a1fff27171ebc1b93d4dc57bceb7ae2430f98a6a4d83d8279ee65d71c1203d2c96d65ebbf7cce9d32971c3de5084cce04a2e147821" |> Utils.hexToBigInt
// let r = BigInteger.Parse "548099063082341131477253921760299949438196259240"
// let s = BigInteger.Parse "857042759984254168557880549501802188789837994940"

let findPrivate q r s digest k : BigInteger =
    match (Utils.modInvBig r q) with 
    | Some r' -> (r' * (s * k - digest)) % q
    | _       -> BigInteger 0

let inputs =
    Utils.readLines "p44_data.txt"
    |> Seq.map (fun (l: string) -> l.Split [|':'|])
    |> Seq.map (fun (a: string []) -> a.[1])
    |> Seq.map (fun (s: string) -> s.[1..])
    |> Seq.toList
    |> List.chunkBySize 4

let rec combs n l = 
    match n, l with
    | 0, _ -> [[]]
    | _, [] -> []
    | k, (x::xs) -> List.map ((@) [x]) (combs (k-1) xs) @ combs k xs
let addReversePairs l =
    let reversed = List.map List.rev l
    l @ reversed

let parseList (l: string list) : (BigInteger * BigInteger * BigInteger)=
    (l.Item(1) |> BigInteger.Parse,
     l.Item(2) |> BigInteger.Parse,
     l.Item(3) |> Utils.hexToBigInt)
    
let compare (l: string list list) : (BigInteger * BigInteger * string list * string list) option = 
    let fst : string list = List.head l
    let snd : string list = List.last l
    let s1, r1, m1 = parseList fst
    let s2, r2, m2 = parseList snd
    let sInv = Utils.modInvBig (s1 - s2) q
    match sInv with
    | None   -> None
    | Some s -> let k = (s * (m1 - m2)) % q
                let x1 = findPrivate q r1 s1 m1 k
                let x2 = findPrivate q r2 s2 m2 k
                let z = BigInteger 0
                if x1 = x2 && x1 > z && x2 > z then Some (k, x1, fst, snd)
                else None

let digestPairs = inputs |> combs 2 |> addReversePairs
let pair = List.map compare digestPairs |> List.filter (fun x -> x <> None)
