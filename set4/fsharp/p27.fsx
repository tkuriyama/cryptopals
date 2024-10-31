#load "utils.fs"

open Utils

let key = Utils.randKey 16 |> Utils.bytesToStr
let iv = key |> Utils.strToBytes

let code : byte [] =
    "comment1=cooking%20MCs;userdata=;comment2=%20like%20a%20pound%20of%20bacon"
    |> Utils.strToBytes
    |> Utils.CBCEncrypt key iv

let decrypt (code: byte []) : byte [] = Utils.CBCDecrypt key iv code 

let check (code: byte []) : string =
    let isValid code =
        let valid (acc: bool) (b: byte) =
            if (int b) > 31 && acc = true then true else false
        Array.fold valid true code
    if isValid (decrypt code) then "" else decrypt code |> Utils.bytesToStr

let checkOriginal : string = check code
let checkHacked : string =
    let e = (Array.length code) - 32
    let zeros = Utils.repeat 0uy |> Seq.take 16 |> Seq.toArray
    Array.concat [| code.[..15]; zeros; code.[..15]; code.[e..] |]
    |> check

let recoveredKey =
    let bs = checkHacked |> Utils.strToBytes
    Utils.xorArr bs.[..16] bs.[32..47]
let test = (key |> Utils.strToBytes) = recoveredKey
