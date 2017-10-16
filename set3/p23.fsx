#load "utils.fs"

open Utils

let u, l = 11, 18
let s, b = 7, 0x9D2C5680u
let t, c = 15, 0xEFC60000u

let getMSB (b: uint32) i : uint32 =
    if i < 0 then 0u else (b >>> (31 - i)) &&& 1u

let RS (acc: uint32) (n: uint32) (i: int) (offset: int) =
    let z = (getMSB n i) ^^^ (getMSB acc (i-offset))
    acc ||| (z <<< (31 - i))
    
let undoRShift (offset: int) (n: uint32) : uint32 =
    List.fold (fun acc i -> RS acc n i offset) 0u [0..31]

let getLSB (b: uint32) i : uint32 =
    if i < 0 then 0u else (b >>> i) &&& 1u

let LS (acc: uint32) (n: uint32) (i: int) (offset: int) (x: uint32) : uint32 =
    let z = (getLSB n i) ^^^ ((getLSB acc (i-offset)) &&& (getLSB x i))
    acc ||| (z <<< i)
    
let undoLShift (offset: int) (x: uint32) (n: uint32) : uint32 =
    List.fold (fun acc i -> LS acc n i offset x) 0u [0..31]

let MTUntemper (n: uint32) : uint32 =
    undoRShift l n
    |> undoLShift t c
    |> undoLShift s b
    |> undoRShift u

let testUntemper =
    MTUntemper (Utils.MTNextValue [|1239001u|]) = 1239001u

let MTSeq = Utils.MTSequence 0u |> Seq.take 1000 |> Seq.toArray
let MTDupSeq = Array.map MTUntemper MTSeq.[1..624] |> Utils.infSeq

let compare =
    (MTSeq.[624..634], Seq.take 10 MTDupSeq |> Seq.toArray)
