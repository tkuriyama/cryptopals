#load "Utils.fs"
open Utils

let code = "1b37373331363f78151b7f2b783431333d78397828372d363c78373e783a393b3736"

let decrypt = Utils.decodeHex code |> Utils.decryptSingleXor
