#load "Utils.fs"
open Utils

let xor (b1: byte list) (b2: byte list) : byte seq =
    Seq.zip b1 b2
    |> Seq.map (fun (x, y) -> x ^^^ y)

let bytesToStr (b: byte seq) : string =
    Seq.map (sprintf "%02x") b
    |> String.concat ""

let s1 = "1c0111001f010100061a024b53535009181c"
let s2 = "686974207468652062756c6c277320657965"
let target = "746865206b696420646f6e277420706c6179"

let test =
    (xor (Utils.decodeHex s1) (Utils.decodeHex s2) |> bytesToStr) = target
