#load "utils.fs"

open Utils
open System

let lines =
    Utils.readLines "p10_problem.txt"
    |> Seq.map Convert.FromBase64String
    |> Seq.concat
    |> Seq.toArray

let key = "YELLOW SUBMARINE"

let testEncryptAES = Utils.prepareTextECB "This is a test!!!!" |> Utils.AESEncryptECB key
let testDecryptAES = Utils.AESDecryptECB key testEncryptAES |> bytesToStr

let testEncryptCBC = Utils.CBCEncrypt key Utils.IV "This is a test!!This is a test!!"
let testDecryptCBC = Utils.CBCDecrypt key Utils.IV testEncryptCBC |> bytesToStr

let decrypted = Utils.CBCDecrypt key Utils.IV lines |> bytesToStr
