module Utils

open System
open System.IO
open System.Numerics
open System.Security.Cryptography

(* IO *)

let readLines (filePath: string) =
    seq { use sr = new StreamReader (filePath)
          while not sr.EndOfStream do
          yield sr.ReadLine () }

(* Helpers *)

let xor (b1: byte seq) (b2: byte seq) : byte seq =
    Seq.zip b1 b2
    |> Seq.map (fun (x, y) -> x ^^^ y)
    
let xorArr b1 b2 = xor b1 b2 |> Array.ofSeq

let xorArrs (lst: byte [] list) : byte [] =
    let rec f acc lst =
        match acc, lst with
            | [||], x::y::[] -> xorArr x y
            | [||], x::y::xs -> f (xorArr x y) xs
            | _,    x::[]    -> xorArr acc x
            | _,    x::xs    -> f (xorArr acc x) xs
    f [||] lst 

let rec transpose xss =
   match xss with
       | ([]::_) -> []
       | xss   -> List.map List.head xss :: transpose (List.map List.tail xss)

let repeat x = seq { while true do yield x }
let repeatArr x n = repeat x |> Seq.take n |> Seq.toArray
let repeatSeq xs = seq { while true do yield! xs }

(* Encodings *)

let hexToByte = function
    | '0' -> 0uy  | '1' -> 1uy
    | '2' -> 2uy  | '3' -> 3uy
    | '4' -> 4uy  | '5' -> 5uy
    | '6' -> 6uy  | '7' -> 7uy
    | '8' -> 8uy  | '9' -> 9uy
    | 'a' -> 10uy | 'b' -> 11uy
    | 'c' -> 12uy | 'd' -> 13uy
    | 'e' -> 14uy | 'f' -> 15uy
    | _ -> failwith "Invalid hex char"

let hexToBytes s =
    Seq.toList s
    |> List.chunkBySize 2 
    |> Seq.map (fun pair -> (Seq.head pair, Seq.last pair))
    |> Seq.map (fun (x, y) -> (hexToByte x <<< 4) ||| hexToByte y)
    |> List.ofSeq

let update i n b =
    ((b |> int |> BigInteger) <<< (i * 8)) ||| n

let bytesToBigInt bs =
    bs
    |> List.rev
    |> List.fold (fun (i, n) b -> (i + 1, update i n b)) (0, BigInteger 0)
    |> snd

let hexToBigInt s = hexToBytes s |> bytesToBigInt

let bytesToStr (b: byte seq) : string =
    Seq.toArray b
    |> Text.Encoding.ASCII.GetString
    
let bytesToHex (b: byte seq) : string =
    Seq.map (sprintf "%02x") b
    |> String.concat ""

let strToBytes (s: string) : byte [] =
    Text.Encoding.ASCII.GetBytes s
    
let histogram xs =
    Seq.groupBy id xs
    |> Map.ofSeq
    |> Map.map (fun k v -> Seq.length v)

(* Modular Inverse *)

let rec egcd (a: int) (b: int) : (int * int * int) =
    match a, b with
    | 0, b -> (b, 0, 1)
    | a, b -> let (g, s, t) = egcd (b % a) a in
              (g, (t - (b / a) * s), s)

let modInv a m : int option =
    let g, s, _ = egcd a m
    let mkPos n = if n < 0 then n + m else n
    if g = 1 then Some (mkPos s) else None

let rec egcdBig (a: BigInteger) (b: BigInteger) : (BigInteger * BigInteger * BigInteger) =
    if a = (BigInteger 0) then (b, (BigInteger 0), (BigInteger 1))
    else let (g, s, t) = egcdBig (b % a) a in (g, (t - (b / a) * s), s)

let modInvBig a m : BigInteger option =
    let g, s, _ = egcdBig a m
    let mkPos n = if n < (BigInteger 0) then n + m else n
    if g = (BigInteger 1) then Some (mkPos s) else None

(* RSA *)

let primes = readLines "large_primes.csv" |> Seq.map BigInteger.Parse

let genRSAKeys (r: Random) : ((BigInteger * BigInteger) * (BigInteger * BigInteger)) =
    let rec pick e =
        let p = Seq.take (1 + r.Next(9999)) primes |> Seq.last
        if p % e = (BigInteger 0) then pick e else p
    let e = BigInteger 3
    let p = pick e
    let q = pick e
    let n = p * q
    let et = (p - (BigInteger 1)) * (q - (BigInteger 1))
    let d = (modInvBig e et).Value
    ((e, n), (d, n))