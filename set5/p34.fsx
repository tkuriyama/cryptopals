
#load "utils.fs"
#load "sha1.fs"

open Utils
open Sha1
open System
open System.Numerics

let p_hex = "ffffffffffffffffc90fdaa22168c234c4c6628b80dc1cd129024e088a67cc74020bbea63b139b22514a08798e3404ddef9519b3cd3a431b302b0a6df25f14374fe1356d6d51c245e485b576625e7ec6f44c42e9a637ed6b0bff5cb6f406b7edee386bfb5a899fa5ae9f24117c4b1fe649286651ece45b3dc2007cb8a163bf0598da48361c55d39a69163fa8fd24cf5f83655d23dca3ad961c62f356208552bb9ed529077096966d670c354e4abc9804f1746c08ca237327ffffffffffffffff"

let p_big = Utils.hexToBigInt p_hex
let g_big = BigInteger 2

let r = new Random()
let a = r.Next(1000) % 37 |> BigInteger
let b = r.Next(1000) % 37 |> BigInteger

(* Normal Exchange *)

let keyAToB (p, g) = (p, g, BigInteger.ModPow (g, a, p))
let keyBToA (p, g, A) = (p, g, A, BigInteger.ModPow (g, b, p))

let msgAToB (p, g, A, B) =
    let s_a = BigInteger.ModPow (B, a, p)
    let key = s_a.ToByteArray() |> Sha1.sha1
    let iv = Utils.randKey 16
    let code =
        "YELLOW SUBMARINE RED SUBMARINE BLUE SUBMARINE"
        |> Utils.strToBytes
        |> Utils.CBCEncrypt key.[..15] iv
    (p, g, A, B, code, iv)

let msgBToA (p, g, A, B, codeA, ivA) = 
    let s_b = BigInteger.ModPow (A, b, p)
    let key = s_b.ToByteArray() |> Sha1.sha1
    let msgA = Utils.CBCDecrypt key.[..15] ivA codeA
    let iv = Utils.randKey 16
    let code =
        Utils.strToBytes " GREEN SUBMARINE"
        |> Array.append msgA 
        |> Utils.CBCEncrypt key.[..15] iv
    (p, g, A, B, code, iv)

let endA (p, g, A, B, codeB, ivB) =
    let s_a = BigInteger.ModPow (B, a, p)
    let key = s_a.ToByteArray() |> Sha1.sha1
    let msgB = Utils.CBCDecrypt key.[..15] ivB codeB
    msgB |> Utils.bytesToStr

let normalExchange = keyAToB (p_big, g_big) |> keyBToA |> msgAToB |> msgBToA |> endA
