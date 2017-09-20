#load "utils.fs"

open Utils
open System
open System.Text

let code =
    ["Um9sbGluJyBpbiBteSA1LjAKV2l0aCBteSByYWctdG9wIGRvd24gc28gbXkg";
     "aGFpciBjYW4gYmxvdwpUaGUgZ2lybGllcyBvbiBzdGFuZGJ5IHdhdmluZyBq";
     "dXN0IHRvIHNheSBoaQpEaWQgeW91IHN0b3A/IE5vLCBJIGp1c3QgZHJvdmUg";
     "YnkK"]
    |> String.concat ""

let oracle =
    Convert.FromBase64String(code)
    |> Utils.ECBOracle false

let verifyBlockSize oracle : int option =
    let rec findKeySize size : int option =
        let keyRepeated : bool =
            let code : byte [] =
                oracle [| for _ in 0 .. size*2 do yield 0 |> byte |]
            code.[0..size-1] = code.[size..size*2-1]
        match size, keyRepeated with
            | 50, _    -> None
            | _, true  -> Some size
            | _, false -> findKeySize (size + 1)  
    findKeySize 2

let blockSize = verifyBlockSize oracle
let verifyECB = Utils.detectECB oracle
let text = Utils.decryptECBOracle oracle blockSize 0 |> Utils.bytesToStr
