#load "Utils.fs"
open Utils

let s1 = "1c0111001f010100061a024b53535009181c"
let s2 = "686974207468652062756c6c277320657965"
let target = "746865206b696420646f6e277420706c6179"

let test =
    let b1 = Utils.decodeHex s1
    let b2 = Utils.decodeHex s2
    (Utils.xor b1 b2 |> Utils.bytesToHex) = target
