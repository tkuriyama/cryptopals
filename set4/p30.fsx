#load "utils.fs"
#load "md4.fs"

open Utils
open MD4

(* Prpare Inputs *)

let key = Utils.randKey 16
let target = ";admin=True" |> Utils.strToBytes
let targetBytes =
    let padding = 64 - (Array.length target) |> Utils.repeatArr 0uy
    Array.append padding target

let originalMsg = "comment1=cooking%20MCs;userdata=foo;comment2=%20like%20a%20pound%20of%20bacon" |> Utils.strToBytes

let preprocessHack (data: byte []) : byte [] =
    let dataPadded = Utils.repeatArr 0uy 16 |> Array.append data
    let pl = MD4.padLen dataPadded
    if pl = 0 then data
    else let zeros = Utils.repeatArr 0uy (pl - 3)
         Array.concat [| data; [|128uy|]; zeros; (MD4.msgLen dataPadded) |]

let targetMsg =
    Array.append (preprocessHack originalMsg) targetBytes

(* MD4-MAC Forgery *)

let MAC k m = Array.append k m |> MD4.md4

let genState (hash: string) =
    let ints = Utils.decodeHex hash |> List.toArray |> MD4.bytesToInts
    (ints.[0], ints.[1], ints.[2], ints.[3])

let forge (hash: string) (msg: byte []) : string =
    MD4.bytesToInts msg
    |> MD4.md4Iter (genState hash)
    |> finalize

let originalMAC = MAC key originalMsg
let targetMAC = MAC key targetMsg
let forgedMAC = forge originalMAC targetBytes
let test = targetMAC = forgedMAC
