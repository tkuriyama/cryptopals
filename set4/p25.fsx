#load "utils.fs"

open Utils
open System

let code =
    readLines "p25_data.txt"
    |> Seq.map Convert.FromBase64String
    |> Seq.concat
    |> Seq.toArray

let text = Utils.AESDecryptECB "YELLOW SUBMARINE" code |> Utils.stripPKCS7 

let key = Utils.randKey 16 |> Utils.bytesToStr
let nonce = Utils.repeat (byte 0) |> Seq.take 8 |> Seq.toArray
let codeCTR = applyCTR key nonce text

let edit (code: byte []) (key: string) (offset: int) (newText: byte []) : byte [] =
    let l = Array.length newText
    let p = applyCTR key nonce code
    Array.concat [| p.[0..(offset-1)]; newText; p.[(offset+l)..] |]
    |> applyCTR key nonce
    
let editAPI (code: byte []) (offset: int) (newText: string) : byte [] =
    edit code key offset (newText |> Utils.strToBytes)

let decrypt =
    let sub =
        Utils.repeat "A"
        |> Seq.take (Array.length codeCTR)
        |> String.concat ""
    editAPI codeCTR 0 sub
    |> Utils.xor (sub |> strToBytes)
    |> Utils.xor codeCTR
    |> Seq.toArray

let testDecrypted = decrypt = text
