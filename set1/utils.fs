module Utils

open System
open System.IO

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

let strToBytes (s: string) : byte [] = Text.Encoding.ASCII.GetBytes s

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

(* Hamming Distance *)

let compareByte ((b1: byte), (b2: byte)) : int =
    let countBits b =
        Seq.sum (seq { for i in 0 .. 7 do
                       yield if (b >>> i) &&& 1uy = 1uy then 1 else 0 })
    countBits (b1 ^^^ b2)

let editDistance (b1: byte seq) (b2: byte seq) : int =
    Seq.zip b1 b2
    |> Seq.fold (fun acc pair -> acc + (compareByte pair)) 0

(* Decrypt single-char XOR *)

let freqMap =
    Map.ofList [('a', 0.0651738); ('b', 0.0124248); ('c', 0.0217339);
                ('d', 0.0349835); ('e', 0.1041442); ('f', 0.0197881);
                ('g', 0.0158610); ('h', 0.0492888); ('i', 0.0558094);
                ('j', 0.0009033); ('k', 0.0050529); ('l', 0.0331490);
                ('m', 0.0202124); ('n', 0.0564513); ('o', 0.0596302);
                ('p', 0.0137645); ('q', 0.0008606); ('r', 0.0497563);
                ('s', 0.0515760); ('t', 0.0729357); ('u', 0.0225134);
                ('v', 0.0082903); ('w', 0.0171272); ('x', 0.0013692);
                ('y', 0.0145984); ('z', 0.0007836); (' ', 0.1918182)]

let singleXorGuesses (code: byte seq) : (string * string) seq  =
    seq{ for b in 00uy .. 255uy do
         let guess = xor code (repeat b) 
         yield ([|b|] |> bytesToStr, guess |> bytesToStr) }

let histogram cs =
    Seq.groupBy id cs
    |> Map.ofSeq
    |> Map.map (fun k v -> Seq.length v)

let scoreGuesses (guesses: (string * string) seq) : ((string * string) * float) seq =
    let lookupKey k v =
        match (Map.tryFind k freqMap) with
        | Some x -> x * (float v)
        | None   -> 0.0
    let score hist = Map.fold (fun s k v -> s + (lookupKey k v)) 0.0 hist
    Seq.map (fun (key, guess) -> ((key, guess), histogram guess |> score)) guesses

let decryptSingleXor code =
    code
    |> singleXorGuesses
    |> scoreGuesses
    |> Seq.maxBy snd

