#load "utils.fs"
#load "sha1.fs"

open Utils
open Sha1

(* Prpare Inputs *)

let key = Utils.randKey 16
let target = ";admin=True" |> Utils.strToBytes
let targetBytes =
    let padding = 64 - (Array.length target) |> Utils.repeatArr 0uy
    Array.append padding target

let originalMsg = "comment1=cooking%20MCs;userdata=foo;comment2=%20like%20a%20pound%20of%20bacon" |> Utils.strToBytes

let preprocessHack (data: byte []) : byte [] =
    let dataPadded = Utils.repeatArr 0uy 16 |> Array.append data
    let pl = Sha1.padLen dataPadded
    if pl = 0 then data
    else let zeros = Utils.repeatArr 0uy (pl - 3)
         Array.concat [| data; [|128uy|]; zeros; (Sha1.msgLen dataPadded) |]

let targetMsg =
    Array.append (preprocessHack originalMsg) targetBytes

(* SHA1-MAC Forgery *)

let MAC k m = Array.append k m |> Sha1.sha1

let genState (hash: string) =
    let ints = Utils.decodeHex hash |> List.toArray |> Sha1.bytesToInts
    (ints.[0], ints.[1], ints.[2], ints.[3], ints.[4])

let forge (hash: string) (msg: byte []) : string =
    Sha1.bytesToInts msg
    |> sha1Iter (genState hash)
    |> finalize

let originalMAC = MAC key originalMsg
let targetMAC = MAC key targetMsg
let forgedMAC = forge originalMAC targetBytes
let test = targetMAC = forgedMAC
