#load "utils.fs"

open Utils
open System

let lines =
    Utils.readLines "p20_data.txt"
    |> Seq.map Convert.FromBase64String
    |> Seq.toList

let key = Utils.randKey 16 |> Utils.bytesToStr
let nonce = Utils.repeat (byte 0) |> Seq.take 8 |> Seq.toArray
let encrypted : byte [] list =
    List.map (fun x -> Utils.applyCTR key nonce x) lines

let keySize : int =
    encrypted |> List.map Array.length |> List.min

let keyGuess : byte [] =
    encrypted 
    |> List.map (fun x -> Seq.take keySize x |> Seq.toList)
    |> List.concat
    |> List.chunkBySize keySize
    |> Utils.transpose
    |> List.map (Utils.decryptSingleXor >> fst >> fst)
    |> String.concat ""
    |> Utils.strToBytes

let solve =
    encrypted
    |> List.map (fun x -> Seq.take keySize x |> Seq.toArray)
    |> List.map (Utils.xorArr keyGuess)
    |> List.map Utils.bytesToStr
