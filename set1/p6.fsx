#load "utils.fs"

open Utils
open System

let testHamming =
    let f = Utils.strToBytes
    Utils.editDistance (f  "this is a test") (f "wokka wokka!!!") = 37

let code =
    readLines "p6_problem.txt"
    |> Seq.map Convert.FromBase64String
    |> Seq.concat
    |> Seq.toList

let avgDistance (len: float) (chunks: byte seq seq) : float =
    Seq.windowed 2 chunks
    |> Seq.map (fun [|x; y|] -> (float (Utils.editDistance x y)) / len)
    |> Seq.average

let findKeySize (guesses: int list) (code: byte seq) (numChunks: int) : int =
    let score len =
        Utils.chunk len code
        |> Seq.take numChunks
        |> avgDistance (float len)
    Seq.map (fun len -> (len, score len)) guesses
    |> Seq.minBy snd
    |> fst

let seqToList (xss: 'a seq seq) : 'a list list =
    Seq.toList xss |> List.map Seq.toList

let findKey code = 
    let keySize = findKeySize [2..40] code 10
    code
    |> Utils.chunk keySize
    |> Seq.take (int (List.length code / keySize))
    |> seqToList
    |> Utils.transpose
    |> List.map (Utils.decryptSingleXor >> fst >> fst)
    |> String.concat ""
    
let solve =
    let key = findKey code
    Utils.xor code (Utils.repeatSeq (Utils.strToBytes key))
    |> Utils.bytesToStr
