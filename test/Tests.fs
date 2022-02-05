module Tests
open FsUnit
open Octopus
open OctocusVariableManager
open VarJsonParser
open Xunit
open Xunit.Abstractions

type``VarJsonParser should``(output:ITestOutputHelper) =

    let write result = output.WriteLine
    let readfile = System.IO.File.ReadAllText
    let equalto = equal

    let mutable MockOctopusVariables = Map[
                                        "fizz", [{ Value = "buzz"; Scope  = None }];  
                                        "fizz2", [{ Value = "newbuzz"; Scope  = None }];
                                       ]

    let GetVariableSet projectName = MockOctopusVariables 
    let UpdateVariableSet projectName (environnmentVariables : Map<string, OctopusVariable list>) = 
        let update key value = MockOctopusVariables <- (MockOctopusVariables |> Map.change key  (fun _ -> Some value) )    
        environnmentVariables |> Map.iter update

    
    [<Fact>]
    let ``Plan should indicate change on existing value`` () =
        Plan "test" (Map ["fizz", "buzz";  "fizz2", "CHANGED";]) GetVariableSet
        |> should be (equalto (Map[
        "fizz2", { OldValue = Some "newbuzz"; NewValue = "CHANGED"};
        ]))

    
    [<Fact>]
    let ``Plan should indicate change on new value`` () =
        Plan "test" ( Map ["something", "new"]) GetVariableSet
        |> should be (equalto (Map[
        "something", { OldValue = None; NewValue = "new"};
        ]))

        
    [<Fact>]
    let ``Can modify octopus variable`` () =
        let environnmentVariables = Map ["fizz", "buzz";  "fizz2", "newbuzz2";]
        UpdateProjectEnvironnmentVariable "test" environnmentVariables UpdateVariableSet
        let (_, value) = GetProjectEnvironnmentVariables "test" GetVariableSet
                            |> Seq.find(fun (key, _) -> key = "fizz")
        value
        |> should be (equalto [{ Value = "buzz"; Scope = None }])
        
    [<Fact>]
    let ``Transform json config into environnement variable`` () =
        "config_sample.json" 
        |> readfile
        |> Parse "PREFIX" 
        |> Map.toList
        |> should be (equalto [
        ("PREFIX_array_0_keyinside", "BOOM");
        ("PREFIX_key1", "value1");
        ("PREFIX_key2", "value2"); 
        ("PREFIX_key3", "False");
        ("PREFIX_key4", "2012-04-23T18:25:43.511Z"); 
        ("PREFIX_key5", "");
        ("PREFIX_key6", "5"); 
        ("PREFIX_key7", "1.33")])
    
    [<Fact>]
    let ``Transform json config into environnement variable without prefix`` () =
        "config_sample.json" 
        |> readfile
        |> Parse "" 
        |> Map.toList
        |> should be (equalto [
        ("array_0_keyinside", "BOOM")
        ("key1", "value1"); 
        ("key2", "value2"); 
        ("key3", "False"); 
        ("key4", "2012-04-23T18:25:43.511Z");
        ("key5", "");
        ("key6", "5");
        ("key7", "1.33");
        ] )

    [<Fact>]
    let ``Display environnement variable`` () =
        "config_sample.json" 
                |> readfile
                |> Parse "PREFIX"
                |> Print
                |> should equalto "PREFIX_array_0_keyinside=BOOM\n\
                                   PREFIX_key1=value1\n\
                                   PREFIX_key2=value2\n\
                                   PREFIX_key3=False\n\
                                   PREFIX_key4=2012-04-23T18:25:43.511Z\n\
                                   PREFIX_key5=\n\
                                   PREFIX_key6=5\n\
                                   PREFIX_key7=1.33\n"