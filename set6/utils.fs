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

let parseHexPair (h: char list) : byte =
    match List.length h with
    | 2 -> ((List.head h |> hexToByte) <<< 4) ||| (List.last h |> hexToByte)
    | _ -> List.head h |> hexToByte 

let hexToBytes s =
    Seq.toList s
    |> List.chunkBySize 2
    |> List.map parseHexPair

let hexBytesToBigInt (bs: byte list) : BigInteger =
    let updateShift i b s = if i = 0 && (int b) < 16 then 4 else 8 |> (+) s
    let update n b s = ((b |> int |> BigInteger) <<< s) ||| n
    let rec loop i bs acc s =
        match bs with
        | x::xs -> loop (i+1) xs (update acc x s) (updateShift i x s)
        | []    -> acc
    loop 0 (List.rev bs) (BigInteger 0) 0

let hexToBigInt s = hexToBytes s |> hexBytesToBigInt

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
    match a = (BigInteger 0) with
    | true -> Some (BigInteger 0)
    | _    -> let g, s, _ = egcdBig a m
              let mkPos n = if n < (BigInteger 0) then n + m else n
              if g = (BigInteger 1) then Some (mkPos s) else None

(* Primes *)

let genBigInt (r: Random) (bits: int) : BigInteger =
    BigInteger 0

let isProbPrime (p: BigInteger) : bool =
    true

let genPrime (bits: int) : BigInteger =
    let rec loop (n: int) (r: Random): BigInteger =
        match n > 1000 with
        | true -> BigInteger 0
        | _    -> let g = genBigInt r bits
                  match isProbPrime g with
                  | true -> g
                  | _    -> loop (n+1) r
    let r = new Random()
    loop 0 r

let primes = readLines "large_primes.csv" |> Seq.map BigInteger.Parse

(* RSA *)

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

(* N Root *)

let rootBig (n: BigInteger) (A: BigInteger) : BigInteger =
    let rec f x tries =
        match tries with
        | 10000 -> x
        | _     -> let m = n - (BigInteger 1)
                   let x' = (m*x + A/(BigInteger.Pow (x, (int m)))) / n
                   match abs(x' - x) with
                   | t when t < (BigInteger 2) -> x'
                   | _ -> f x' (tries+1)
    f (A / n) 0

(* DSA *)

let genDSAKeys (rand: Random) =
    let L = 256
    let N = 40
    let q = "f4f47f05794b256174bba6e9b396a7707e563c5b" |> hexToBigInt
    let p = "800000000000000089e1855218a0e7dac38136ffafa72eda7859f2171e25e65eac698c1702578b07dc2a1076da241c76c62d374d8389ea5aeffd3226a0530cc565f3bf6b50929139ebeac04f48c3c84afb796d61e5a4f9a8fda812ab59494232c7d2b4deb50aa18ee9e132bfa85ac4374d7f9091abc3d015efc871a584471bb1" |> hexToBigInt
    let g = "5958c9d3898b224b12672c0b98e06c60df923cb8bc999d119458fef538b8fa4046c8db53039db620c094c9fa077ef389b5322a559946a71903f990f1f7e0e025e2d7f7cf494aff1a0470f5b64c36b625a097f1651fe775323556fe00b3608c887892878480e99041be601a62166ca6894bdd41a7054ec89f756ba9fc95302291" |> hexToBigInt
    let x = 1 + rand.Next(1000000000) |> BigInteger
    let pub = BigInteger.ModPow (g, x, p)
    (x, pub, q, p, g)

let signDSA x q p g k digest : (BigInteger * BigInteger) option =
    let r = (BigInteger.ModPow (g, k, p)) % q
    match (modInvBig k q) with
    | Some k' -> let s = (k' * (digest + x * r)) % q
                 Some (r, s)
    | _       -> None

let rec signDSARandK x q p g digest (rand: Random) : (BigInteger * BigInteger * BigInteger) =
    let k = 1 + rand.Next(1000000000) |> BigInteger
    match (signDSA x q p g k digest) with
    | Some (r, s) -> r, s, k
    | None        -> signDSARandK x q p g digest rand

let verifyDSA q p g y digest r s : bool = 
    let reject n = n < (BigInteger 0) || n > q
    let verify =
        match (modInvBig s q) with
        | Some w -> let u1 = (digest * w) % q
                    let u2 = (r * w) % q
                    let g' = BigInteger.ModPow (g, u1, p)
                    let y' = BigInteger.ModPow (y, u2, p)
                    let v = ((g' * y') % p) % q
                    v = r
        | _      -> false
    if reject r || reject s then false else verify
