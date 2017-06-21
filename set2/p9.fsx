#load "utils.fs"

open Utils

let str = "YELLOW SUBMARINE" |> Utils.strToBytes

let padded = Utils.padPKCS7 20 str
let paddedStr = padded |> Utils.bytesToStr
