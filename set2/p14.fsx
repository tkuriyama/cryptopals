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
    |> Utils.ECBOracleOffset true

let decrypt oracle (blockSize: int option) : byte [] =
    let rec tryDecrypt noiseSize oracle blockSize =
        match noiseSize, blockSize with
            | _, None   -> [||]
            | 10, _     -> [||]
            | n, Some b -> let offset = (float n) / (float b) |> ceil |> int
                           let noise = [| for _ in [1..n] do yield byte 0 |]
                           let text = decryptECBOracle (oracle noise) blockSize offset
                           printfn "%A" text
                           if Utils.valid text (Array.length text) then text 
                           else tryDecrypt (n + 1) oracle blockSize
    tryDecrypt 0 oracle blockSize

decrypt oracle (Some 16) |> Utils.bytesToStr
