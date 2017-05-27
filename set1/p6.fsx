#load "utils.fs"

open Utils
open System

let ascii (s: string) : byte [] = Text.Encoding.ASCII.GetBytes s

let testHamming =
    Utils.editDistance (ascii  "this is a test") (ascii "wokka wokka!!!") = 37

let lines = readLines "p6_problem.txt"

    
