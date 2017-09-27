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

(* padding oracle attack *)

let genGuesses (code: byte []) ind1 offset (found: byte []): byte [] list =  
    let genGuess n : byte [] =
        Utils.xorArrs [ [|code.[ind1+offset]|]; [|byte n|]; [|byte (16-offset)|] ] 
        |> Array.ofSeq
    [ for i in [0..255] do yield Array.concat [| code.[ind1..(ind1+offset-1)];
                                                 genGuess i;
                                                 found;
                                                 code.[(ind1+16)..(ind1+31)] |] ]
     
let rec evalGuesses ind (guesses: byte [] list) : (byte * byte []) =
    match guesses with
        | x::[] -> (x.[ind], x)
        | x::xs -> if paddingOracle x = true then (x.[ind], x)
                   else evalGuesses ind xs 
        | _     -> (0uy, [||])

let genBlock (bs: byte []) n =
    Utils.xorArr [|for _ in [0..n] do yield byte n |] bs.[(16-n)..]
    |> Array.ofSeq

let disambiguate (b: byte, bs: byte []) : byte [] =
    let perturb b = Utils.xorArr [|b|] [|1uy|] 
    let rec checkOracle guesses =
        match guesses with
            | []        -> [|b|]
            | (n,x)::xs -> if paddingOracle x = false then checkOracle xs
                           else genBlock x n
    [ for i in [1..15] do
          yield (i+1, Array.concat [| bs.[0..(15-i-1)];
                                      perturb bs.[(15-i)];
                                      bs.[(15-i+1)..15] |]) ]
    |> checkOracle

let decryptLastByte code ind1 : byte [] =
    genGuesses code ind1 15 [||]
    |> evalGuesses (ind1+31)
    |> disambiguate     

let decryptByte code ind1 offset found : (byte * byte []) =
    genGuesses code ind1 offset found
    |> evalGuesses (ind1+16+offset)

let rec decryptBlock ind1 offset (found: byte []) (code: byte []): byte [] =
    match offset with
        | -1 -> found
        | 15 -> let bs = decryptLastByte code ind1
                let n = Array.length bs
                decryptBlock ind1 (offset-n) (Array.append bs found) code
        | _  -> let b, _ = decryptByte code ind1 offset found
                decryptBlock ind1 (offset-1) (Array.append [|b|] found) code

let CBCPaddingDecrypt (code: byte []) =
    let numBlocks = Array.chunkBySize 16 code |> Array.length
    [| for i in [0..(numBlocks-1)] do
           yield Array.append iv code |> decryptBlock (i*16) 15 [||] |]
    |> Array.concat

CBCPaddingDecrypt testEncrypt

