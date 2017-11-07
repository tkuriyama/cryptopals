#load "utils.fs"
#load "sha1.fs"

open Utils
open Sha1
open System
open System.Numerics
open System.Security.Cryptography

let N_hex = "ffffffffffffffffc90fdaa22168c234c4c6628b80dc1cd129024e088a67cc74020bbea63b139b22514a08798e3404ddef9519b3cd3a431b302b0a6df25f14374fe1356d6d51c245e485b576625e7ec6f44c42e9a637ed6b0bff5cb6f406b7edee386bfb5a899fa5ae9f24117c4b1fe649286651ece45b3dc2007cb8a163bf0598da48361c55d39a69163fa8fd24cf5f83655d23dca3ad961c62f356208552bb9ed529077096966d670c354e4abc9804f1746c08ca237327ffffffffffffffff"

let SHA256ToInt (bs: byte []) : int =
    (new SHA256Managed()).ComputeHash bs
    |> Array.fold (fun acc x -> acc + (int x)) 0

(* Client and Server Pre-Agreed *) 

let N = BigInteger 37
let g = BigInteger 2
let k = BigInteger 3
let I = "test@test.com" |> Utils.strToBytes
let P = "YELLOW SUBMARINE" |> Utils.strToBytes

(* Secrets *)

let r = new Random()
let a = r.Next(100) |> BigInteger
let b = r.Next(100) |> BigInteger

(* Server Computation *) 

let salt = r.Next(1000) |> BigInteger
let v =
    let x = Array.append (salt.ToByteArray()) P |> SHA256ToInt
    BigInteger.ModPow (g, BigInteger x, N)

(* Exchange *)

let keyCToS = (I, BigInteger.ModPow (g, a, N))
let keySToC (I, A) = (I, A, salt, (k * v + BigInteger.ModPow (g, b, N)))

let HMACCToS (I, A: BigInteger, salt: BigInteger, B: BigInteger) =
    let u: int = Array.append (A.ToByteArray()) (B.ToByteArray()) |> SHA256ToInt
    let x: int = Array.append (salt.ToByteArray()) P |> SHA256ToInt
    let s: BigInteger = BigInteger.ModPow ((B - k * BigInteger.Pow(g, x)),
                                           (a + (BigInteger (u * x))),
                                           N)
    let K = (new SHA256Managed()).ComputeHash (s.ToByteArray()) 
    let h = (new HMACSHA256(K)).ComputeHash (salt.ToByteArray())
    (I, A, salt, B, h)

let HMACSToC (I, A: BigInteger, salt: BigInteger, B: BigInteger, h) =
    let u: int = Array.append (A.ToByteArray()) (B.ToByteArray()) |> SHA256ToInt
    let s: BigInteger = BigInteger.ModPow (A * BigInteger.Pow(v, u), b, N)
    let K = (new SHA256Managed()).ComputeHash (s.ToByteArray())
    let h' = (new HMACSHA256(K)).ComputeHash (salt.ToByteArray())
    if h = h' then "ok" else "hashes do not match"

let exchange = keyCToS |> keySToC |> HMACCToS |> HMACSToC
