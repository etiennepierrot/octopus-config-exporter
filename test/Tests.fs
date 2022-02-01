module Tests

open System
open FsUnit
open Xunit
open Xunit.Abstractions
open FSharp.Data

type Tests(output:ITestOutputHelper) =


    let write result =
        output.WriteLine (sprintf "The actual result was %O" result)
    let rec Parse  (value :JsonValue) (key :string) (acc) = 
        let newKey x =  (key + "_" + x) 
        match value with
        | JsonValue.String s -> (key, s) :: acc
        | JsonValue.Boolean b -> (key, b |> string ) :: acc
        | JsonValue.Record r -> r |> Array.map(fun (x,y) ->  Parse y  (newKey x) acc ) |> List.concat
        | JsonValue.Array a ->  a |> Array.mapi(fun i e -> Parse e (newKey (i |> string)) acc ) |> List.concat
        | _ -> (key, "error") :: acc


    [<Fact>]
    let ``My test`` () =
        let json = System.IO.File.ReadAllText "config_sample.json"
        let data = JsonValue.Parse(json)
        let result = Parse data "PREFIX" List.empty
        result |> should be (equal [("PREFIX_key1", "value1"); ("PREFIX_key2", "value2"); ("PREFIX_key3", "false"); ("PREFIX_array_0_keyinside", "BOOM")])
   