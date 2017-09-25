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

let decryptByte code ind1 ind2 offset =
    1

let rec decryptBlock (code: byte []) ind1 ind2 offset (found: byte []) : byte [] =
    match offset with
        | -1 -> found |> Array.rev
        | _  -> let b = decryptByte code ind1 ind2 offset
                decryptBlock code (ind1+1) (ind2+1) (offset-1) (Array.append found [|b|])

let CBCPaddingDecrypt (code: byte []) =
    let numBlocks = Array.chunkBySize 16 code |> Array.length
    [| for i in [0..(numBlocks-2)] do yield
           decryptBlock code (i*16) ((i+1)*16) 15 []|]
    |> Array.concat
    
