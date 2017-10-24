#load "utils.fs"

open Utils

(* SHA-1 *)

let h0 = 0x67452301u
let h1 = 0xEFCDAB89u
let h2 = 0x98BADCFEu
let h3 = 0x10325476u
let h4 = 0xC3D2E1F0u
let initState = (h0, h1, h2, h3, h4)

let leftRotate x (n: uint32) : uint32 =
    (n <<< x) ||| (n >>> (32 - x))

let preprocess (data: byte []) : uint32 [] =
    [||]

let rec extend (ws: uint32 []) i : uint32 [] =
    match i with
    | 80 -> ws
    | _  -> let w = ws.[i-3] ^^^ ws.[i-8] ^^^ ws.[i-14] ^^^ ws.[i-16] |> leftRotate 1
            extend (Array.append ws [|w|]) (i+1)

let rec applyIter (ws: uint32 []) (state: uint32*uint32*uint32*uint32*uint32) i =
    match i with
    | 80 -> state
    | _  -> let a, b, c, d, e = state
            let f, k =
                if i <= 19 then ((b &&& c) ||| (~~~b &&& d), 0x5A827999u)
                elif i <= 39 then (b ^^^ c ^^^ d, 0x6ED9EBA1u)
                elif i <= 59 then ((b &&& c) ||| (b &&& d) ||| (c &&& d), 0x8F1BBCDCu)
                else (b ^^^ c ^^^ d, 0xCA62C1D6u)
            let temp = (leftRotate 5 a) + f + e + k + ws.[i]
            applyIter ws (temp, a, leftRotate 30 b, c, d) (i+1)

let sha1Iter (state: uint32*uint32*uint32*uint32*uint32) (chunk: uint32 []) =
    let words = extend chunk 16
    let h0, h1, h2, h3, h4 = state
    let a, b, c, d, e = applyIter words state 0
    (h0+a, h1+b, h2+c, h3+d, h4+e)

let finalize state =
    0

let sha1 (data: byte []) =
    preprocess data
    |> Array.chunkBySize 16
    |> Array.fold (fun state c -> sha1Iter state c) initState
    |> finalize
