#load "utils.fs"
#load "sha1.fs"

open Utils
open Sha1

let key = "YELLOW SUBMARINE" |> Utils.strToBytes
let key2 = "YELLOW SUBMARINE2" |> Utils.strToBytes

let message = "This is a secret message yo" |> Utils.strToBytes

let MAC = Array.append key message |> Sha1.sha1
let MAC2 = Array.append key2 message |> Sha1.sha1
