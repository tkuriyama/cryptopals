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

let genGuesses (code: byte []) s offset (found: byte []): (byte * byte []) list =
    let guessByte n =
        Utils.xorArr [|code.[s+offset]|] [|byte n|] 
    let padding n = Utils.repeat (byte n) |> Seq.take (n-1) |> Seq.toArray
    let foundBytes =
        Utils.xorArrs [ code.[s+offset+1..s+15]; padding (16-offset); found ] 
    let genGuess i: byte [] =
        Array.concat [| code.[s..(s+offset-1)];
                        guessByte i; foundBytes;
                        code.[(s+16)..(s+31)] |]
    [ for i in [0..255] do yield (byte i, genGuess i) ]

let rec evalGuesses ind (guesses: (byte * byte []) list) : (byte * byte []) =
    match guesses with
        | (b,x)::[] -> (b, x)
        | (b,x)::xs -> if paddingOracle x = true then (b, x)
                       else evalGuesses ind xs 
        | _         -> (0uy, [||])

let genBlock (bs: byte []) n : byte [] =
    [|for _ in [0..(n-1)] do yield byte n |] |> Array.ofSeq

let disambiguate (b: byte, bs: byte []) : byte [] =
    let rec check pairs =
        match pairs with
            | []       -> Utils.xorArr [|b|] [|1uy|]
            | (n,x)::_ -> if paddingOracle x = false then genBlock x n
                          else check (List.tail pairs)
    [ for i in [1..15] do
          yield (17-i, Array.concat [| bs.[0..(i-2)];
                                       Utils.xorArr [|bs.[i-1]|] [|1uy|] 
                                       bs.[i..] |]) ]
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
    |> Utils.xorArr [|byte (16 - offset)|]

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

let text = CBCPaddingDecrypt testEncrypt |> Utils.stripPKCS7 |> Utils.bytesToStr
