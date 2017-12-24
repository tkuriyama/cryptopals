#load "utils.fs"
#load "sha1.fs"

open Utils
open Sha1
open System
open System.Numerics

let text = "hi mom" |> Utils.strToBytes 
let digest = Sha1.sha1 text

let signature =
    let r = 128 - 3 - ((String.length digest) / 2)
    String.concat "" ["0001"; String.replicate r "ff"; "00"; digest]
