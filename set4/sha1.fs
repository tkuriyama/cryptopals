#load "utils.fs"

open Utils

let h0 = 0x67452301u
let h1 = 0xEFCDAB89u
let h2 = 0x98BADCFEu
let h3 = 0x10325476u
let h4 = 0xC3D2E1F0u
let initState = (h0, h1, h2, h3, h4)

let leftRotate s (n: uint32) : uint32 =
    (n <<< s) ||| (n >>> (32 - s))

let padLen (data: byte []) : int =
    (64 - ((Array.length data) % 64)) % 64

let msgLen (data: byte []) : byte [] =
    let getByte n s = n >>> (8 * s) |> byte
    let l = Array.length data |> (*) 8
    [| getByte l 1; getByte l 0 |]

let bytesToInts (data: byte []): uint32 [] =
    let rec convert (bytes: byte []) n s : uint32 =
        if s = 0 then (n + uint32 bytes.[0])
        else convert bytes.[1..] ((n + (uint32 bytes.[0])) <<< 8) (s-8)
    [| for i in [0..4..((Array.length data) - 1)] do
       yield convert data.[i..i+3] (uint32 0) 24 |]

let preprocess (data: byte []) : byte [] =
    let pl = padLen data
    if pl = 0 then data
    else let zeros = Utils.repeatArr 0uy (pl - 3)
         Array.concat [| data; [|128uy|]; zeros; (msgLen data) |]

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

let int32ToHex (n: uint32) =
    let h1 = n >>> 24
    let h2 = (n <<< 8) >>> 24
    let h3 = (n <<< 16) >>> 24
    let h4 = (n <<< 24) >>> 24
    List.map (sprintf "%02x") [h1; h2; h3; h4]
    |> String.concat ""

let finalize state : string =
    let h0, h1, h2, h3, h4 = state
    Array.map int32ToHex [| h0; h1; h2; h3; h4 |]
    |> String.concat ""

let sha1 (data: byte []) =
    preprocess data
    |> bytesToInts
    |> Array.chunkBySize 16
    |> Array.fold (fun state c -> sha1Iter state c) initState
    |> finalize

let test =
    "The quick brown fox jumps over the lazy cog"
    |> Utils.strToBytes
    |> sha1
    |> (=) "de9f2c7fd25e1b3afad3e85a0bd17d9b100db4b3"
