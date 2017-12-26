#load "utils.fs"
#load "sha1.fs"

open Utils
open Sha1
open System
open System.Numerics

let ((e, n), (d, _)) = Utils.genRSAKeysSample

let text = "hi mom" |> Utils.strToBytes 
let digest = Sha1.sha1 text
let block =
    let r = 128 - 3 - ((String.length digest) / 2)
    String.concat "" ["0001"; String.replicate r "ff"; "00"; digest]
let signature : byte [] =
    let s = Utils.hexToBigInt block |> Utils.decryptRSA d n
    s.ToByteArray()

let verify (s: byte []) (digestLen: int) : bool =
    let s' = Utils.encryptRSA e n (BigInteger s)
    let sHex = s'.ToByteArray() |> Array.rev |> Array.append [|0uy|]
               |> Utils.bytesToHex
    let i = (String.length sHex) - digestLen - 4
    if sHex.[0..3] = "0001" && sHex.[(i..(i+3)] = "ff00" then true else false
let testVerify = verify signature 40
