open Utils

let base16To64 (s: string): string =
    Utils.decodeHex s
    |> Seq.toArray
    |> System.Convert.ToBase64String
   
let input = "49276d206b696c6c696e6720796f757220627261696e206c696b65206120706f69736f6e6f7573206d757368726f6f6d"
let target = "SSdtIGtpbGxpbmcgeW91ciBicmFpbiBsaWtlIGEgcG9pc29ub3VzIG11c2hyb29t"

let test = (base16To64 input) = target
