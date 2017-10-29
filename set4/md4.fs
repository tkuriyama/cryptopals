module MD4

open System

let strToBytes (s: string) : byte [] =
    Text.Encoding.ASCII.GetBytes s

let repeat x = seq { while true do yield x }
let repeatArr x n = repeat x |> Seq.take n |> Seq.toArray

let a = 0x67452301u
let b = 0xefcdab89u
let c = 0x98badcfeu
let d = 0x10325476u
let initState = (a, b, c, d)

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
    else let zeros = repeatArr 0uy (pl - 3)
         Array.concat [| data; [|128uy|]; zeros; (msgLen data) |]

let F x y z = (x &&& y) ||| (~~~x &&& z)
let G x y z = (x &&& y) ||| (x &&& z) ||| (y &&& z)
let H x y z = x ^^^ y ^^^ z

let rec round (state: uint32*uint32*uint32*uint32) f (inds: int []) add (chunk: uint32 []) n =
    if n = 4 then state
    else let lr = leftRotate inds.[8+n]
         let a, b, c, d = state
         let aa = a + (f b c d) + chunk.[inds.[n] + inds.[4]] + add |> lr
         let dd = d + (f aa b c) + chunk.[inds.[n] + inds.[5]] + add |> lr
         let cc = c + (f dd aa b) + chunk.[inds.[n] + inds.[6]] + add |> lr
         let bb = b + (f cc dd aa) + chunk.[inds.[n] + inds.[7]] + add |> lr
         round (aa, bb, cc, dd) f inds add chunk (n+1)

let md4Iter (state: uint32*uint32*uint32*uint32) (chunk: uint32 []) =
    let aa, bb, cc, dd = state
    let rec iter n s =
        match n with
        | 0 -> round s F [| 0; 4; 8; 12; 0; 1; 2; 3; 3; 7; 11; 19 |] (uint32 0) chunk 0
               |> iter (n+1)
        | 1 -> round s G [| 0; 1; 2; 3; 0; 4; 8; 12; 3; 5; 9; 13 |] 0x5a827999u chunk 0
               |> iter (n+1)
        | 2 -> round s H [| 0; 2; 1; 3; 0; 8; 4; 12; 3; 9; 11; 15 |] 0x6ed9eba1u chunk 0
               |> iter (n+1)
        | _ -> let a, b, c, d = s in (a+aa, b+bb, c+cc, d+dd)
    iter 0 state

let int32ToHex (n: uint32) =
    let h1 = n >>> 24
    let h2 = (n <<< 8) >>> 24
    let h3 = (n <<< 16) >>> 24
    let h4 = (n <<< 24) >>> 24
    List.map (sprintf "%02x") [h1; h2; h3; h4]
    |> String.concat ""

let finalize state : string =
    let a, b, c, d = state
    Array.map int32ToHex [| a; b; c; d |]
    |> String.concat ""

let md4 (data: byte []) =
    preprocess data
    |> bytesToInts
    |> Array.chunkBySize 16
    |> Array.fold md4Iter initState
    |> finalize

let md4Test =
    "The quick brown fox jumps over the lazy cog"
    |> strToBytes
    |> md4
    |> (=) "b86e130ce7028da59e672d56ad0113df"
