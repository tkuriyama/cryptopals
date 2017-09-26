#load "utils.fs"

open Utils
open System
open System.Text

let strings =
    [|"MDAwMDAwTm93IHRoYXQgdGhlIHBhcnR5IGlzIGp1bXBpbmc=";
      "MDAwMDAxV2l0aCB0aGUgYmFzcyBraWNrZWQgaW4gYW5kIHRoZSBWZWdhJ3MgYXJlIHB1bXBpbic=";
      "MDAwMDAyUXVpY2sgdG8gdGhlIHBvaW50LCB0byB0aGUgcG9pbnQsIG5vIGZha2luZw==";
      "MDAwMDAzQ29va2luZyBNQydzIGxpa2UgYSBwb3VuZCBvZiBiYWNvbg==";
      "MDAwMDA0QnVybmluZyAnZW0sIGlmIHlvdSBhaW4ndCBxdWljayBhbmQgbmltYmxl";
      "MDAwMDA1SSBnbyBjcmF6eSB3aGVuIEkgaGVhciBhIGN5bWJhbA==";
      "MDAwMDA2QW5kIGEgaGlnaCBoYXQgd2l0aCBhIHNvdXBlZCB1cCB0ZW1wbw==";
      "MDAwMDA3SSdtIG9uIGEgcm9sbCwgaXQncyB0aW1lIHRvIGdvIHNvbG8=";
      "MDAwMDA4b2xsaW4nIGluIG15IGZpdmUgcG9pbnQgb2g=";
      "MDAwMDA5aXRoIG15IHJhZy10b3AgZG93biBzbyBteSBoYWlyIGNhbiBibG93"|]

let key = Utils.randKey 16 |> Utils.bytesToStr
let iv = Utils.IV

let testEncrypt =
    let rnd = Random()
    strings.[rnd.Next 9] |> Convert.FromBase64String |> Utils.CBCEncrypt key iv

let paddingOracle (code: byte []) : bool = 
    code |> Utils.CBCDecryptKeepPad key iv |> Utils.validPKCS7
    
let testDecrypt = testEncrypt |> paddingOracle

// padding oracle attack

let genGuesses code ind1 offset: byte [] [] = 
    let genGuess n =
        Utils.xor [|code.[ind1+offset]|] [|byte n|]
        |> Utils.xor [|byte (16-offset)|]
        |> Array.ofSeq
    [| for i in [0..255] do yield
           Array.concat [| code.[ind1..(ind1+offset-1)];
                           [| genGuess n |];
                           found;
                           code.[(ind1+16)..(ind1+31) |] |]
     
let rec evalGuesses ind guesses : byte =
    match guesses with
        | x::[] -> x.[ind]
        | x::xs -> if paddingOracle x is true then x.[ind]
                   else evalGuesses offset xs 

let disambiguate code ind1 b : byte [] =
    [||]

let decryptLastByte code ind1 =
    genGuesses code ind1 15 [||]
    |> evalGuesses (ind1+31)
    |> disambiguate code ind1 
    
let genPadding n = [| for n in [1..n] do yield byte n |]

let decryptByte code ind1 offset found =
    genGuesses code ind1 offset found
    |> evalGuesses (ind1+16+offset)

let rec decryptBlock ind1 offset (found: byte []) (code: byte []): byte [] =
    match offset with
        | -1 -> found
        | 15 -> let bs = decryptLastByte code ind1 ind2
                let n = Array.length b
                decryptBlock code ind1 (offset-n) (Array.append bs found)
        | _  -> let b = decryptByte code ind1 ind2 offset found
                decryptBlock code ind1 (offset-1) (Array.append [|b|] found)

let CBCPaddingDecrypt (code: byte []) =
    let numBlocks = Array.chunkBySize 16 code |> Array.length
    [| for i in [0..(numBlocks-1)] do yield
           Array.append iv code |> decryptBlock (i*16) 15 [] |]
    |> Array.concat
