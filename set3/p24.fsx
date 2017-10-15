#load "utils.fs"

open Utils

let code = "Yellow submarine, testing testing" 

let applyCTR (seed: uint32) (code: byte []) : byte [] =
    Utils.xorArr (MTSequence seed |> Seq.map byte) code

let encrypted = Utils.strToBytes code |> applyCTR 0u
let decrypted = applyCTR 0u encrypted |> Utils.bytesToStr
