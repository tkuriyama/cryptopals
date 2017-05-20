let hexToByte = function
    | '0' -> 0uy  | '1' -> 1uy
    | '2' -> 2uy  | '3' -> 3uy
    | '4' -> 4uy  | '5' -> 5uy
    | '6' -> 6uy  | '7' -> 7uy
    | '8' -> 8uy  | '9' -> 9uy
    | 'a' -> 10uy | 'b' -> 11uy
    | 'c' -> 12uy | 'd' -> 13uy
    | 'e' -> 14uy | 'f' -> 15uy
    | _ -> failwith "Invalid hex char"

let chunk n xs =
    xs 
    |> Seq.mapi (fun i x -> i/n, x)
    |> Seq.groupBy fst
    |> Seq.map (fun (_, g) -> Seq.map snd g)

let decodeHex s =
    s
    |> chunk 2
    |> Seq.map (fun pair -> (Seq.head pair, Seq.last pair))
    |> Seq.map (fun (x, y) -> (hexToByte x <<< 4) ||| hexToByte y)
    |> List.ofSeq

let xor (b1: byte list) (b2: byte list) : byte seq =
    Seq.zip b1 b2
    |> Seq.map (fun (x, y) -> x ^^^ y)

let bytesToStr (b: byte seq) : String =
    Seq.map (sprintf "%02x") b
    |> String.concat ""

let s1 = "1c0111001f010100061a024b53535009181c"
let s2 = "686974207468652062756c6c277320657965"
let target = "746865206b696420646f6e277420706c6179"

let test =
    (xor (decodeHex s1) (decodeHex s2) |> bytesToStr) = target
