#load "utils.fs"

open Utils

let profileParse (s: string) =
    let elems = s.Split([|'&'; '='|])
    let e = Array.length elems - 1
    [| for i in [0..e]
           do if i % 2 = 0 then yield (elems.[i], elems.[i+1]) |]
    |> Map.ofArray

let encodeEmail (email: string) =
    let sanitized =
        let elems = email.Split([|'@'; '&'; '='|])
        if Array.length elems > 2 then elems.[0] else email
    let s = String.concat "" ["email="; sanitized; "&uid=10&role=user"]
    profileParse s

let profile_for (email: string) =
    let profile = encodeEmail email
    String.concat "" ["email="; profile.["email"];
                      "&uid="; profile.["uid"];
                      "&role="; profile.["role"]]        

let key = Utils.randKey 16 |> Utils.bytesToStr
let iv = Utils.randKey 16
let encryptProfile key iv profile =
    profile |> Utils.strToBytes |> Utils.AESEncryptECB key iv

let encrypted = profile_for "test@test.com" |> encryptProfile key iv
let decrypted = Utils.AESDecryptECB key encrypted |> Utils.bytesToStr


let genAdmin =
    let p1 = profile_for "test@test.com" |> encryptProfile key iv
    let p2 = profile_for "aaaaaaaaaaadmin" |> encryptProfile key iv
    let e = Array.length p1 - 17
    Array.concat [| p1.[..e]; p2.[16..31] |]

let adminProfile =
    genAdmin |> Utils.AESDecryptECB key |> Utils.bytesToStr
