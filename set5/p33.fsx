#load "utils.fs"

open Utils
open System
open System.Numerics
open System.Globalization

let r = new Random()

let p, g = bigint 37, bigint 5

let a = r.Next(1000) % 37
let A = (pown g a) % p

let b = r.Next(1000) % 37
let B = (pown g b) % p

let s_a = (pown B a) % p
let s_b = (pown A b) % p

let p_hex = "ffffffffffffffffc90fdaa22168c234c4c6628b80dc1cd129024e088a67cc74020bbea63b139b22514a08798e3404ddef9519b3cd3a431b302b0a6df25f14374fe1356d6d51c245e485b576625e7ec6f44c42e9a637ed6b0bff5cb6f406b7edee386bfb5a899fa5ae9f24117c4b1fe649286651ece45b3dc2007cb8a163bf0598da48361c55d39a69163fa8fd24cf5f83655d23dca3ad961c62f356208552bb9ed529077096966d670c354e4abc9804f1746c08ca237327ffffffffffffffff"

let p_big = Utils.hexToBigInt p_hex
let g_big = BigInteger 2

let A_big = BigInteger.ModPow (g, (BigInteger a), p_big)
let B_big = BigInteger.ModPow (g, (BigInteger b), p_big)
let s_a_big = BigInteger.ModPow (B_big, BigInteger a, p_big)
let s_b_big = BigInteger.ModPow (A_big, BigInteger b, p_big)
