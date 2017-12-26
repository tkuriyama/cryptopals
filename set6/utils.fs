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

let rec genRSAKeys (r: Random) : ((BigInteger * BigInteger) * (BigInteger * BigInteger)) =
    let rec pick e s n =
        let p = Seq.take (s + r.Next(n)) primes |> Seq.last
        if p % e = (BigInteger 0) then pick e s n else p
    let e = BigInteger 3
    let p = pick e 1 4998
    let q = pick e 5000 4999
    let n = p * q
    let et = (p - (BigInteger 1)) * (q - (BigInteger 1))
    let d = modInvBig e et
    match d with
    | Some v -> ((e, n), (v, n))
    | _      -> genRSAKeys r

let genRSAKeysSample =
    let p = "ffffffffffffffffc90fdaa22168c234c4c6628b80dc1cd129024e088a67cc74020bbea63b139b22514a08798e3404ddef9519b3cd3a431b302b0a6df25f14374fe1356d6d51c245e485b576625e7ec6f44c42e9a637ed6b0bff5cb6f406b7edee386bfb5a899fa5ae9f24117c4b1fe649286651ece45b3dc2007cb8a163bf0598da48361c55d39a69163fa8fd24cf5f83655d23dca3ad961c62f356208552bb9ed529077096966d670c354e4abc9804f1746c08ca237327ffffffffffffffff" |> hexToBigInt
    let e = BigInteger 3
    let q = BigInteger 961957691
    let n = p * q
    let et = (p - (BigInteger 1)) * (q - (BigInteger 1))
    let d = modInvBig e et
    ((e, n), (d.Value, n))

let encryptRSA (e: BigInteger) (n: BigInteger) (m: BigInteger) =
    if m > n then failwith "message out of range"
    else BigInteger.ModPow (m, e, n)

let decryptRSA (d: BigInteger) (n: BigInteger) (c: BigInteger) =
    if c > n then failwith "message out of range"
    else BigInteger.ModPow (c, d, n)
