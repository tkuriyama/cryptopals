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
    |> Utils.ECBOracle true

let decrypt oracle =
    let rec trydecrypt n oracle blockSize =
        match n with
            | 32  -> [||]
            | _   -> let text = decryptECBOracle oracle blockSize n
                     if Utils.valid text then tryDecrypt (n + 1) oracle blockSize
                     else text |> Utils.bytesToStr
    tryDecrypt 0 oracle 16 [||]
