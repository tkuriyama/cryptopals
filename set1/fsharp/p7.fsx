#load "utils.fs"

open Utils
open System

let code =
    Utils.readLines "p7_problem.txt"
    |> Seq.map Convert.FromBase64String
    |> Seq.concat
    |> Seq.toArray

let key = "YELLOW SUBMARINE"

let decrypted = Utils.aesDecrypt code key
