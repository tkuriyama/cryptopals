#load "utils.fs"

open Utils
open System

let lines =
    Utils.readLines "p8_problem.txt"
    |> Seq.map Utils.decodeHex

let detect =
    Seq.filter detectECB lines
