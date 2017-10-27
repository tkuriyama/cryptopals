#load "sha1.fs"

open Sha1

let key = "YELLOW SUBMARINE" |> Sha1.strToBytes
let key2 = "YELLOW SUBMARINE2" |> Sha1.strToBytes

let message = "This is a secret message yo" |> Sha1.strToBytes

let MAC = Array.append key message |> Sha1.sha1
let MAC2 = Array.append key2 message |> Sha1.sha1
