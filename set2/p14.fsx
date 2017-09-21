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

let pairs =
    [ for offset in [0..2] do for noise in [0..15] do
          yield (offset, noise) ]

let rec decrypt oracle pairs (blockSize: int option) : byte [] =
    match pairs, blockSize with
        | _,  None -> [||]
        | [], _    -> [||]
        | (o, n)::xs, Some b ->
            let noise = [| for _ in [1..n] do yield byte 0 |]
            let text = decryptECBOracle (oracle noise) blockSize o
            if Utils.valid text (Array.length text) then text 
            else decrypt oracle xs blockSize

let text = decrypt oracle pairs (Some 16) |> Utils.bytesToStr

