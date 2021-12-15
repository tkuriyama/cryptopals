#load "utils.fs"

open Utils
open System

let lines =
    Utils.readLines "p10_problem.txt"
    |> Seq.map Convert.FromBase64String
    |> Seq.concat
    |> Seq.toArray

let key = "YELLOW SUBMARINE"

let input1 = "This is a test!!!!"
let testEncryptAES = input1 |> strToBytes |> Utils.AESEncryptECB key [||]
let testDecryptAES = Utils.AESDecryptECB key testEncryptAES |> bytesToStr

let input2 = "This is a test!!This is a test!!"
let testEncryptCBC = input2 |> strToBytes |> Utils.CBCEncrypt key Utils.IV
let testDecryptCBC = Utils.CBCDecrypt key Utils.IV testEncryptCBC |> bytesToStr

let decrypted = Utils.CBCDecrypt key Utils.IV lines |> bytesToStr
