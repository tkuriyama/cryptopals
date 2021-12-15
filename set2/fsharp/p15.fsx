#load "utils.fs"

open Utils

let valid = Array.append ("ICE ICE BABY" |> Utils.strToBytes) [|4uy; 4uy; 4uy; 4uy|]
let validStripped = Utils.stripPKCS7 valid |> Utils.bytesToStr

let invalid = Array.append ("ICE ICE BABY" |> Utils.strToBytes) [|1uy; 2uy; 3uy; 4uy|]
let invalidStripped = Utils.stripPKCS7 invalid |> Utils.bytesToStr
