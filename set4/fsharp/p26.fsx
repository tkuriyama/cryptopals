#load "utils.fs"

open Utils


let key = Utils.randKey 16 |> Utils.bytesToStr
let nonce = Utils.repeat (byte 0) |> Seq.take 8 |> Seq.toArray

let encrypt (input: string) : byte [] =
    let pre = "comment1=cooking%20MCs;userdata=" |> Utils.strToBytes
    let post = ";comment2=%20like%20a%20pound%20of%20bacon" |> Utils.strToBytes
    let text = input.Replace("=", "'='").Replace(";", "';'") |> Utils.strToBytes
    Array.concat [|pre; text; post|]
    |> Utils.applyCTR key nonce

let isAdmin (text: string) : bool =
    text.Split(';') |> Set.ofArray |> Set.contains "admin=True"

let decrypt (code: byte []) : byte [] = Utils.applyCTR key nonce code 

let checkAdmin s = 
    printfn "\nstring: %s" s
    printfn "admin in string: %b\n" (isAdmin s)
    s

let rec hackByte i (code: byte []) (target: byte) (guesses: int list) : byte =
    match guesses with
        | []    -> 0uy
        | x::[] -> byte x
        | x::xs -> let guess = Array.concat[|code.[0..(i-1)]; [|byte x|] ; code.[(i+1)..]|]
                   let decrypted = decrypt guess
                   if decrypted.[i] = target then byte x
                   else hackByte i code target xs
        
let hack (map: Map<int,byte>) (code : byte []) : byte [] =
    let applyHack i b =
        if Map.containsKey i map then hackByte i code (Map.find i map) [1..255]
        else b
    Array.mapi applyHack code

let test = encrypt "test;admin=True" |> decrypt |> bytesToStr |> checkAdmin

let targetMap = Map.ofList [(32, 59uy); (38, 61uy); (43, 59uy)]
let hacked = encrypt "ZadminZTrueZ" |> hack targetMap |> decrypt |> bytesToStr |> checkAdmin
