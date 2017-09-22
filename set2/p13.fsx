#load "utils.fs"

open Utils

let profileParse (s: string) =
    let pairs = s.Split([|'&'; '='|])
    let e = Array.length pairs - 1
    [| for i in [0..e]
           do if i % 2 = 0 then yield (pairs.[i], pairs.[i+1]) |]
    |> Map.ofArray

let encodeEmail (email: string) =
    let s = String.concat "" ["email="; email; "&uid=10&role=user"]
    profileParse s
                       
