#load "utils.fs"

open System
open Utils

let plainText = "Burning 'em, if you ain't quick and nimble\nI go crazy when I hear a cymbal"
let plainBytes = Text.Encoding.ASCII.GetBytes plainText
let key = "ICE"
let target = "0b3637272a2b2e63622c2e69692a23693a2a3c6324202d623d63343c2a26226324272765272a282b2f20430a652e2c652a3124333a653e2b2027630c692b20283165286326302e27282f"

let keyFull = seq {while true do yield! (Text.Encoding.ASCII.GetBytes key) }
let encrypted = Utils.xor plainBytes keyFull

let test = Utils.bytesToHex encrypted = target
