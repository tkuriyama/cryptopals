#load "Utils.fs"

open Utils
open System.IO

let readLines (filePath: string) =
    seq { use sr = new StreamReader (filePath)
          while not sr.EndOfStream do
          yield sr.ReadLine () }

let solve =
    readLines "p4_problem.txt"
    |> Seq.map Utils.decodeHex
    |> Seq.map Utils.decryptSingleXor
    |> Seq.maxBy (fun tup -> snd tup)
