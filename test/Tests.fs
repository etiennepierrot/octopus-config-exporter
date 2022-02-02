module Tests

open System
open FsUnit
open VarJsonParser
open Xunit
open Xunit.Abstractions

type``VarJsonParser should``(output:ITestOutputHelper) =

    let write result = output.WriteLine
    let readfile = System.IO.File.ReadAllText
    let equalto = equal

    [<Fact>]
    let ``Transform Json Config Into Environnement Variable`` () =
        "config_sample.json" 
        |> readfile
        |> Parse "PREFIX" 
        |> should be (equalto [
        ("PREFIX_key1", "value1"); 
        ("PREFIX_key2", "value2"); 
        ("PREFIX_key3", "False"); 
        ("PREFIX_key4", "2012-04-23T18:25:43.511Z");
        ("PREFIX_key5", "");
        ("PREFIX_key6", "5");
        ("PREFIX_key7", "1.33");
        ("PREFIX_array_0_keyinside", "BOOM")
        ])

    [<Fact>]
    let ``Display Environnement Variable`` () =
        "config_sample.json" 
                |> readfile
                |> Parse "PREFIX"
                |> Print
                |> should equalto "PREFIX_key1=value1\n\
                                   PREFIX_key2=value2\n\
                                   PREFIX_key3=False\n\
                                   PREFIX_key4=2012-04-23T18:25:43.511Z\n\
                                   PREFIX_key5=\n\
                                   PREFIX_key6=5\n\
                                   PREFIX_key7=1.33\n\
                                   PREFIX_array_0_keyinside=BOOM\n"