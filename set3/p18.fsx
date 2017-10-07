#load "utils.fs"

open Utils
open System.Text

let code =
    "L77na/nrFsKvynd6HzOoG7GHTLXsTVu9qvY/2syLXzhPweyyMTJULu/6/kXX0KSvoOLSFQ=="
    |> Convert.FromBase64String

let nonce = Utils.repeat (byte 0) |> Seq.take 8 |> Seq.toArray
let decoded = applyCTR "YELLOW SUBMARINE" nonce code |> Utils.bytesToStr
