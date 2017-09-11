#load "utils.fs"

open Utils
open System.Text

let code =
    ["Um9sbGluJyBpbiBteSA1LjAKV2l0aCBteSByYWctdG9wIGRvd24gc28gbXkg";
     "aGFpciBjYW4gYmxvdwpUaGUgZ2lybGllcyBvbiBzdGFuZGJ5IHdhdmluZyBq";
     "dXN0IHRvIHNheSBoaQpEaWQgeW91IHN0b3A/IE5vLCBJIGp1c3QgZHJvdmUg"]
    |> String.concat ""

let oracle =
    Convert.FromBase64String(code)
    |> Utils.ECBOracle

let verifyECB = detectECB oracle


