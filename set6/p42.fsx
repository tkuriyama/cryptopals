#load "utils.fs"
#load "sha1.fs"

open Utils
open Sha1
open System
open System.Numerics
open System.Text.RegularExpressions

let ((e, n), (d, _)) = Utils.genRSAKeysSample

let text = "hi mom" |> Utils.strToBytes 
let digest = Sha1.sha1 text
let block =
    let r = 128 - 3 - ((String.length digest) / 2)
    String.concat "" ["0001"; String.replicate r "ff"; "00"; digest]
let signature : byte [] =
    let s = Utils.hexToBigInt block |> Utils.decryptRSA d n
    s.ToByteArray()

let (|ParseRegex|_|) regex str =
   let m = Regex(regex).Match(str)
   if m.Success
   then Some (List.tail [ for x in m.Groups -> x.Value ])
   else None

let parseSignature s : bool =
    match s with
    | ParseRegex "0001fff*00.*" s -> true
    | _ -> false

let genSignature (s: byte []) : string =  
    let s' = Utils.encryptRSA e n (BigInteger s)
    s'.ToByteArray() |> Array.rev |> Array.append [|0uy|]
    |> Utils.bytesToHex
let verify (s: byte []) : bool = genSignature s |> parseSignature
let testVerify = verify signature

let forge (digest: string) : byte [] =
    let r = 128 - 4 - ((String.length digest) / 2)
    let s =
        String.concat "" ["0001ff00"; digest; String.replicate r "00"]
        |> Utils.hexToBigInt
        |> Utils.rootBig (BigInteger 3)
    s.ToByteArray()
let testForge = forge digest |> verify
