module Utils

open System
open System.IO
open System.Security.Cryptography

(* IO *)

let readLines (filePath: string) =
    seq { use sr = new StreamReader (filePath)
          while not sr.EndOfStream do
          yield sr.ReadLine () }

(* Helpers *)

let xor (b1: byte seq) (b2: byte seq) : byte seq =
    Seq.zip b1 b2
    |> Seq.map (fun (x, y) -> x ^^^ y)

let rec transpose xss =
   match xss with
       | ([]::_) -> []
       | xss   -> List.map List.head xss :: transpose (List.map List.tail xss)

let repeat x = seq { while true do yield x }
let repeatSeq xs = seq { while true do yield! xs }

(* Encodings *)

let hexToByte = function
    | '0' -> 0uy  | '1' -> 1uy
    | '2' -> 2uy  | '3' -> 3uy
    | '4' -> 4uy  | '5' -> 5uy
    | '6' -> 6uy  | '7' -> 7uy
    | '8' -> 8uy  | '9' -> 9uy
    | 'a' -> 10uy | 'b' -> 11uy
    | 'c' -> 12uy | 'd' -> 13uy
    | 'e' -> 14uy | 'f' -> 15uy
    | _ -> failwith "Invalid hex char"

let decodeHex s =
    Seq.toList s
    |> List.chunkBySize 2 
    |> Seq.map (fun pair -> (Seq.head pair, Seq.last pair))
    |> Seq.map (fun (x, y) -> (hexToByte x <<< 4) ||| hexToByte y)
    |> List.ofSeq

let bytesToStr (b: byte seq) : string =
    Seq.toArray b
    |> Text.Encoding.ASCII.GetString
    
let bytesToHex (b: byte seq) : string =
    Seq.map (sprintf "%02x") b
    |> String.concat ""

let strToBytes (s: string) : byte [] =
    Text.Encoding.ASCII.GetBytes s
    
let histogram xs =
    Seq.groupBy id xs
    |> Map.ofSeq
    |> Map.map (fun k v -> Seq.length v)

(* PKCS7 Padding *)

let padPKCS7 (length: int) (bytes: byte seq) : byte [] =
    let targetLen = length - (Seq.length bytes % length)
    let padLen = if targetLen > 0 then targetLen else Seq.length bytes
    let pad = repeat (byte padLen)
    Seq.append bytes (Seq.take padLen pad) |> Seq.toArray

let stripPKCS7 (length: int) (code: byte []) : byte [] =
    code
        
(* AES *)

let genAES (key: string) =
    let aes = new AesManaged()
    aes.Mode <- CipherMode.ECB
    aes.Key <- strToBytes key
    aes.Padding <- PaddingMode.None
    aes

let prepareInputECB (input: byte []) : byte [] [] =
    input
    |> padPKCS7 16
    |> Array.chunkBySize 16

let applyAESEncryptECB (key: string) (input: byte [] []) : byte [] =
    use aes = genAES key
    let encryptor = aes.CreateEncryptor(aes.Key, aes.IV)
    [| for block in input do
           let encrypted = Array.create 16 0uy
           encryptor.TransformBlock(block, 0, 16, encrypted, 0) |> ignore
           yield! encrypted |]

let applyAESDecryptECB (key: string) (code: byte []) : byte [] =
    let aes = genAES key
    let decryptor = aes.CreateDecryptor(aes.Key, aes.IV)
    let codeLen = Array.length code
    let decrypted = Array.create codeLen 0uy
    decryptor.TransformBlock(code, 0, codeLen, decrypted, 0) |> ignore
    decrypted

let AESEncryptECB (key: string) (iv: byte []) (input: byte []) : byte [] =
    applyAESEncryptECB key (prepareInputECB input)

let AESDecryptECB (key: string) (code: byte []) : byte [] =
    applyAESDecryptECB key code
    
(* CBC *)

let IV = Seq.take 16 (repeat (byte 0)) |> Seq.toArray

let prepareInputCBC (input: byte []) : byte [] list  =
    input
    |> padPKCS7 16
    |> Array.chunkBySize 16
    |> Array.toList

let prepareCodeCBC(code: byte []) : byte [] list =
    code
    |> stripPKCS7 16
    |> Array.chunkBySize 16
    |> Array.toList

let rec applyCBCEncrypt blocks key acc : byte [] list =
    let genArray = Seq.toArray >> Array.create 1
    let encrypt x y = xor x y |> genArray |> applyAESEncryptECB key
    match blocks with
        | x::y::[] -> let encrypted = encrypt x y
                      List.rev (encrypted::acc)
        | x::y::xs -> let encrypted = encrypt x y
                      applyCBCEncrypt (encrypted::xs) key (encrypted::acc)

let rec applyCBCDecrypt blocks key acc : byte [] list =
    let decrypt x y = applyAESDecryptECB key y |> Array.ofSeq |> xor x |> Seq.toArray
    match blocks with
        | x::y::[] -> let decrypted = decrypt x y
                      List.rev (decrypted::acc)
        | x::y::xs -> let decrypted = decrypt x y
                      applyCBCDecrypt (y::xs) key (decrypted::acc)

let CBCEncrypt (key: string) (iv: byte []) (input: byte []) : byte [] = 
    let blocks = iv :: (prepareInputCBC input)
    applyCBCEncrypt blocks key [] |> Array.concat
   
let CBCDecrypt (key: string) (iv: byte []) (code: byte []) : byte [] =
    let blocks = iv :: (prepareCodeCBC code)
    applyCBCDecrypt blocks key [] |> Array.concat

(* Encryption Oracle *)

let detectECBOutput (code: byte []) : bool =
    Array.chunkBySize 16 code
    |> histogram
    |> Map.toSeq
    |> Seq.fold (fun acc x -> if ((snd x) > 2 || acc) then true else false) false

let detectECB encrypt : bool =
    [| for _ in 1 .. 64 do yield byte 0 |]
    |> encrypt
    |> detectECBOutput

let randKey (size: int) : byte [] =
    let rnd = Random()
    [|for _ in 1..size do yield rnd.Next 256 |> byte|]

let ECBCBCOracle (rnd: Random) =
    let key = randKey 16 |> bytesToStr
    let IV = randKey 16
    let pre = randKey (5 + rnd.Next 6)
    let post = randKey (5 + rnd.Next 6)
    match rnd.Next 2 with
        | 0 -> let ECB code = AESEncryptECB key IV (Array.concat [|pre; code; post|])
               ECB
        | _ -> let CBC code = CBCEncrypt key IV (Array.concat [|pre; code; post|])
               CBC

(* ECB Oracle and byte-at-a-time Decryption *)

let ECBOracle (rand: bool) (post: byte []) =
    let rnd = new Random()
    let key = randKey 16 |> bytesToStr
    let IV = randKey 16
    let pre = if rand then randKey (1 + rnd.Next 15) else [||]
    let ECB code = AESEncryptECB key IV (Array.concat [|pre; code; post|])
    ECB

let ECBOracleOffset (rand: bool) (post: byte []) =
    let rnd = new Random()
    let key = randKey 16 |> bytesToStr
    let IV = randKey 16
    let pre = if rand then randKey (1 + rnd.Next 15) else [||]
    let ECB noise code = AESEncryptECB key IV (Array.concat [|pre; noise; code; post|])
    ECB

let genMap oracle (guess: byte []) blockSize : Map<byte [], byte> =
    let output n = Array.append guess [|byte n|] |> oracle
    let takeFirst (arr: byte []) = arr.[..(blockSize - 1)]
    [for n in [0 .. 255] do yield (output n |> takeFirst, byte n)]
    |> Map.ofList

let decodeChar (code: byte []) blockInd guessMap blockSize : byte =
    let blockStart = blockInd * blockSize
    let blockEnd = blockStart + (blockSize - 1)
    try
        Map.find code.[blockStart..blockEnd] guessMap
    with
        | :? System.Collections.Generic.KeyNotFoundException ->
                 byte 0

let rec decodeBlock oracle blockInd (prev: byte []) (guess: byte []) ind blockSize : byte []  =
    if ind = (blockSize + 1) then guess
    else let guessMap = genMap oracle guess.[1..] blockSize
         let c = decodeChar (oracle prev.[ind..]) blockInd guessMap blockSize
         let newGuess = Array.append guess.[1..] [|c|]
         decodeBlock oracle blockInd prev newGuess (ind + 1) blockSize

let rec decodeBlocks oracle numBlocks blockSize (prev: byte []) found offset : byte [] list =
    let ind = List.length found |> (+) offset
    if ind = numBlocks then found |> List.rev
    else let b = decodeBlock oracle ind prev prev 1 blockSize 
         decodeBlocks oracle numBlocks blockSize b (b::found) offset

let stripPadding lastBlock (arr: byte []) : byte [] =
    [| for b in arr.[lastBlock..] do if int b > 1 then yield b |] 
    |> Array.append arr.[..(lastBlock - 1)]

let valid (text: byte []) : bool =
    [| for b in text do if int b > 1 then yield b |]
    |> Array.length
    |> (>) <| 0

let decryptECBOracle oracle blockSize offset : byte [] =
    match blockSize with
        | None   -> [||]
        | Some n -> let numBlocks = oracle [||] |> Array.length |> (/) <| n
                    let prevBlock = repeat (byte 0) |> Seq.take n |> Seq.toArray
                    decodeBlocks oracle numBlocks n prevBlock [] offset
                    |> List.toArray
                    |> Array.concat
                    |> stripPadding ((numBlocks - 1) * n)
