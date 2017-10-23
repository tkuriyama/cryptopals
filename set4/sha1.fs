#load "utils.fs"

open Utils

(* SHA-1 *)

let h0 = 0x67452301
let h1 = 0xEFCDAB89
let h2 = 0x98BADCFE
let h3 = 0x10325476
let h4 = 0xC3D2E1F0
let initState = (h0, h1, h2, h3, h4)

let preprocess (data: byte []) : byte [] =
    [||]

let sha1Iter (state: int * int * int * int * int)  (chunk: byte []) =
    (0, 0, 0, 0, 0)

let finalize state =
    0

let sha1 (data: byte []) =
    preprocess data
    |> Array.chunkBySize 16
    |> Array.fold (fun state c -> sha1Iter state c) initState
    |> finalize
