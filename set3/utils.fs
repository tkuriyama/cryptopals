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
    
let xorArr b1 b2 = xor b1 b2 |> Array.ofSeq

let xorArrs (lst: byte [] list) : byte [] =
    let rec f acc lst =
        match acc, lst with
            | [||], x::y::[] -> xorArr x y
            | [||], x::y::xs -> f (xorArr x y) xs
            | _,    x::[]    -> xorArr acc x
            | _,    x::xs    -> f (xorArr acc x) xs
    f [||] lst 

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

let validPKCS7 (code: byte []) : bool =
    let arr = Array.rev code
    let rec strip (arr: byte []) padding ctr : bool =
        let c = int arr.[0]
        if c <> padding then false
        elif ctr < padding then strip arr.[1..] padding (ctr + 1)
        else true
    match arr.[0] with
        | 0uy -> false
        | _   -> strip arr (int arr.[0]) 1

let stripPKCS7 (code: byte []) : byte [] =
    let arr = Array.rev code
    let rec strip (arr: byte []) padding ctr : byte [] =
        let c = int arr.[0]
        if c <> padding then failwith "bad padding"
        elif ctr < padding then strip arr.[1..] padding (ctr + 1)
        else arr.[1..] |> Array.rev
    strip arr (int arr.[0]) 1
        
(* AES *)

let randKey (size: int) : byte [] =
    let rnd = Random()
    [|for _ in 1..size do yield rnd.Next 256 |> byte|]

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
    applyCBCDecrypt blocks key [] |> Array.concat |> stripPKCS7

let CBCDecryptKeepPad (key: string) (iv: byte []) (code: byte []) : byte [] =
    let blocks = iv :: (prepareCodeCBC code)
    applyCBCDecrypt blocks key [] |> Array.concat

(* CTR Mode *)

let genCtr ind : byte [] =
    Array.init 8 (fun i -> let n = (i+1) * 256 - 1
                           if n - ind > 255 then byte 0
                           elif ind / n >= 1 then byte 255
                           else byte (ind % 256))

let rec genStream (key: string) (nonce: byte []) numBlocks ind acc : byte [] =
    if ind = numBlocks then acc
    else let enc = genCtr ind |> Array.append nonce |> AESEncryptECB key [||]
         genStream key nonce numBlocks (ind+1) (Array.append acc enc)

let applyCTR (key: string) (nonce: byte []) (code: byte []) : byte [] =
    let numBlocks = Array.chunkBySize 16 code |> Array.length
    genStream key nonce numBlocks 0 [||]
    |> xorArr code
