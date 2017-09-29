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

let genGuesses (code: byte []) s offset (found: byte []): byte [] list =
    let target = 16-offset
    let guessByte n =
        Utils.xorArrs [ [|code.[s+offset]|]; [|byte n|]; [|byte (target)|] ]
    let foundBytes =
        Utils.xorArrs [ code.[s+offset+1..s+15];
                        Utils.repeatArr (byte target) (target-1);
                        found ]
    let genGuess n : byte [] =
        Array.append (guessByte n) foundBytes |> Array.ofSeq
    [ for i in [0..255] do yield Array.concat [| code.[s..(s+offset-1)];
                                                 genGuess i
                                                 code.[(s+16)..(s+31)] |] ]
     
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
    let rec check pairs =
        match pairs with
            | []        -> [|b|]
            | (n,x)::_ -> if paddingOracle x = false then check (List.tail pairs)
                          else genBlock x n        
    [ for i in [1..15] do
          yield (i+1, Array.concat [| bs.[0..(15-i-1)];
                                      Utils.xorArr [|bs.[(15-i)]|] [|1uy|] 
                                      bs.[(15-i+1)..15] |]) ]
    |> check

let decryptLastByte code start : byte [] =
    genGuesses code start 15 [||]
    |> evalGuesses 15
    |> disambiguate     

let decryptByte code start offset found : byte [] =
    genGuesses code start offset found
    |> evalGuesses offset
    |> fst
    |> Array.create 1

let rec decryptBlock start offset (found: byte []) (code: byte []): byte [] =
    match offset with
        | -1 -> found
        | 15 -> let bs = decryptLastByte code start
                decryptBlock start (offset-(Array.length bs)) bs code
        | _  -> let b = decryptByte code start offset found
                decryptBlock start (offset-1) (Array.append b found) code

let CBCPaddingDecrypt (code: byte []) =
    let numBlocks = Array.chunkBySize 16 code |> Array.length |> (+) -1
    [| for i in [0..numBlocks] do
           yield Array.append iv code |> decryptBlock (i*16) 15 [||] |]
    |> Array.concat

let text = CBCPaddingDecrypt testEncrypt |> Utils.bytesToStr
