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

let AESEncryptECB (key: string) (input: byte []) : byte [] =
    applyAESEncryptECB key (prepareInputECB input)

let AESDecryptECB (key: string) (code: byte []) : byte [] =
    applyAESDecryptECB key code
    
(* CBC *)

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

let IV = Seq.take 16 (repeat (byte 0)) |> Seq.toArray

let CBCEncrypt (key: string) (iv: byte []) (input: byte []) : byte [] = 
    let blocks = iv :: (prepareInputCBC input)
    applyCBCEncrypt blocks key [] |> Array.concat
   
let CBCDecrypt (key: string) (iv: byte []) (code: byte []) : byte [] =
    let blocks = iv :: (prepareCodeCBC code)
    applyCBCDecrypt blocks key [] |> Array.concat
    
let detectECB (code: byte []) : bool =
    Array.chunkBySize 16 code
    |> histogram
    |> Map.toSeq
    |> Seq.fold (fun acc x -> if ((snd x) > 2 || acc) then true else false) false

