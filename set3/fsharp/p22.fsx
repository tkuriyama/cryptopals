#load "utils.fs"

open Utils
open System
open System.Threading

let findUnixTime = 
    let dateTime = DateTime.Now
    let epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    (dateTime.ToUniversalTime() - epoch).TotalSeconds

let routineSeed : uint32 =
    let r = System.Random()
    Thread.Sleep(r.Next(40, 1000) * 1000)
    let seed = uint32 findUnixTime
    printfn "Seed: %d" seed
    Thread.Sleep(r.Next(40, 1000) * 1000)
    MTInit seed |> MTNextValue

let crack : (uint32 option) =
    let t = uint32 findUnixTime
    let seedMap =
        [for d in [0..2500] do
         yield let seed = t - (uint32 d) in 
               (MTInit seed |> MTNextValue, seed)]
        |> Map.ofList
    Map.tryFind routineSeed seedMap
