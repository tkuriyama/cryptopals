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
                       
