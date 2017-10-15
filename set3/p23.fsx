#load "utils.fs"

open Utils

let u, l = 11, 18
let s, b = 7, 0x9D2C5680
let t, c = 15, 0xEFC60000

let getMSB (b: uint32) i : uint32 =
    if i < 0 then 0u
    else (b >>> (31 - i)) &&& 1u

let RS (acc: uint32) (n: uint32) (i: int) (offset: int) =
    let z = (getMSB n i) ^^^ (getMSB acc (i-offset))
    acc ||| (z <<< (31 - i))
    
let undoRShift (offset: int) (n: uint32) : uint32 =
    List.fold (fun acc i -> RS acc n i offset) 0u [0..31]

let MTUntemper (uint32: n) : uint32 =
    undoRShift l n
    |> undoLShiftXor t c
    |> undoLShiftXor s b
    |> undoRShift u
