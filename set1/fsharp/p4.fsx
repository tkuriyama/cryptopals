#load "Utils.fs"

open Utils

let solve =
    Utils.readLines "p4_problem.txt"
    |> Seq.map (Utils.decodeHex >> Utils.decryptSingleXor)
    |> Seq.maxBy (snd)
