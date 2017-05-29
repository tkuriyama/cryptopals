#load "utils.fs"

open Utils

let str = "YELLOW SUBMARINE" |> Utils.strToBytes

let padded = Utils.padPKCS7 str 20
let paddedStr = padded |> Utils.bytesToStr
