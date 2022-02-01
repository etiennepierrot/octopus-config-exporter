module Tests

open System
open Xunit
open FSharp.Data



[<Fact>]
let ``My test`` () =
    let json = System.IO.File.ReadAllText "config_sample.json"
    let data = JsonValue.Parse(json)
    printfn "%A" data

    Assert.NotNull(json)
