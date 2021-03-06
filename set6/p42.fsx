#load "utils.fs"
#load "sha1.fs"

open Utils
open Sha1
open System
open System.Numerics
open System.Text.RegularExpressions

let rand = new Random()
let ((e, n), (d, _)) = Utils.genRSAKeys rand 1024

(* Original Signature *)

let text = "hi mom" |> Utils.strToBytes 
let digest = Sha1.sha1 text
let block =
    let r = 128 - 3 - ((String.length digest) / 2)
    String.concat "" ["0001"; String.replicate r "ff"; "00"; digest]
let signature : byte [] =
    let s = Utils.hexToBigInt block |> Utils.decryptRSA d n
    s.ToByteArray()

(* Parser and Verifier *)

let (|ParseRegex|_|) regex str =
   let m = Regex(regex).Match(str)
   if m.Success
   then Some (List.tail [ for x in m.Groups -> x.Value ])
   else None

let parseSignature text s : bool =
    let digest = Sha1.sha1 text
    let regex = String.concat "" [| "0001fff*00"; digest; ".*" |]
    match s with
    | ParseRegex regex s -> true
    | _ -> false

let genSignature (s: byte []) : string =  
    let s' = Utils.encryptRSA e n (BigInteger s)
    s'.ToByteArray() |> Array.rev |> Array.append [|0uy|]
    |> Utils.bytesToHex
    
let verify (text: byte []) (s: byte []) : bool =
    genSignature s |> parseSignature text

let testVerify = verify signature text

(* Forgery *)

let forge (digest: string) : byte [] =
    let r = 128 - 4 - ((String.length digest) / 2)
    let s =
        String.concat "" ["0001ff00"; digest; String.replicate r "00"]
        |> Utils.hexToBigInt
        |> Utils.rootBig (BigInteger 3)
        |> (+) (BigInteger 1)
    s.ToByteArray()

let forgeGuess padLen digest : byte [] =
    let rand = new Random()
    let pad = [| for _ in [1..padLen] do yield byte (rand.Next(256)) |]
              |> Utils.bytesToHex
    let s =
        String.concat "" ["0001ff00"; digest; pad]
        |> Utils.hexToBigInt
        |> Utils.rootBig (BigInteger 3)
    s.ToByteArray()

let forge' (digest: string) : byte [] =
    let padLen = 128 - 4 - ((String.length digest) / 2)
    let rec loop tries =
        let guess = forgeGuess padLen digest
        match tries with
        | 5000 -> guess
        | _     -> match verify text guess with
                   | true -> guess
                   | _    -> loop (tries + 1)
    loop 0

let testForge = forge digest |> verify text
let testForge' = forge' digest |> verify text
