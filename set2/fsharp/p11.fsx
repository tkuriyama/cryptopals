#load "utils.fs"

open Utils
open System

let tests =
    [ for _ in 1 .. 1000 do
      yield if detectECB (ECBCBCOracle (new Random())) then 1 else 0]
    |> List.sum
