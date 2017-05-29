module Utils

open System
open System.IO
open System.Security.Cryptography

(* IO *)

let readLines (filePath: string) =
    seq { use sr = new StreamReader (filePath)
          while not sr.EndOfStream do
          yield sr.ReadLine () }


(* Helpers *)

let chunk n xs =
    xs 
    |> Seq.mapi (fun i x -> i/n, x)
    |> Seq.groupBy fst
    |> Seq.map (fun (_, g) -> Seq.map snd g)

let xor (b1: byte seq) (b2: byte seq) : byte seq =
    Seq.zip b1 b2
    |> Seq.map (fun (x, y) -> x ^^^ y)

let rec transpose xss =
   match xss with
       | ([]::_) -> []
       | xss   -> List.map List.head xss :: transpose (List.map List.tail xss)

let repeat x = seq { while true do yield x }
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

let decodeHex s =
    chunk 2 s
    |> Seq.map (fun pair -> (Seq.head pair, Seq.last pair))
    |> Seq.map (fun (x, y) -> (hexToByte x <<< 4) ||| hexToByte y)
    |> List.ofSeq

let bytesToStr (b: byte seq) : string =
    Seq.toArray b
    |> Text.Encoding.ASCII.GetString
    
let bytesToHex (b: byte seq) : string =
    Seq.map (sprintf "%02x") b
    |> String.concat ""

let strToBytes (s: string) : byte [] = Text.Encoding.ASCII.GetBytes s

(* PKCS7 Padding *)

let padPKCS7 (bytes: byte seq) (length: int) : byte seq =
    let targetLen = length - Seq.length bytes
    let padLen = if targetLen > 0 then targetLen else Seq.length bytes
    let pad = repeat (byte padLen)
    Seq.append bytes (Seq.take padLen pad)
        
