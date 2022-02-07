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
                                        {Key = "fizz"; Scope  = None}, "buzz" ;  
                                        {Key = "fizz2"; Scope  = None}, "newbuzz";  
                                        {Key = "fizz2"; Scope  = Some "QA"}, "newbuzz-QA";  
                                       ]

    let GetVariableSet projectName = MockOctopusVariables 
    let UpdateVariableSet (environnmentVariables :Map<ScopedKey, string>) = 
        let update key value = MockOctopusVariables <- (MockOctopusVariables |> Map.change key  (fun _ -> Some value) )    
        environnmentVariables |> Map.iter update

        
    [<Fact>]
    let ``Plan should indicate change on existing value`` () =
        Plan "test" (Map ["fizz", "buzz"; "fizz2", "CHANGED";]) None GetVariableSet
        |> should be (equalto (Map[
        { Key = "fizz2"; Scope  = None}, Modify { OldValue = "newbuzz"; NewValue = "CHANGED"};
        ]))
    
    
    [<Fact>]
    let ``Plan should indicate change on new value`` () =
        Plan "test" ( Map ["something", "new"]) None  GetVariableSet
        |> should be (equalto (Map[
        { Key = "something"; Scope  = None}, New "new";
        ]))
        
    [<Fact>]
    let ``Can modify octopus variable`` () =
        let environnmentVariables = Map ["fizz", "buzz";  "fizz2", "newbuzz2";]
        UpdateProjectEnvironnmentVariable "test" environnmentVariables None UpdateVariableSet GetVariableSet|> ignore
        let (_, value) = GetProjectEnvironnmentVariables "test" GetVariableSet
                            |> Seq.find(fun (key, _) -> key =  {Key =  "fizz"; Scope = None; })
        value
        |> should be (equalto "buzz")
        
    [<Fact>]
    let ``Transform json config into environnement variable`` () =
        "config_sample.json" 
        |> readfile
        |> Parse (Some "PREFIX") 
        |> Map.toList
        |> should be (equalto [
        ("PREFIX_array__0__keyinside", "BOOM");
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
        |> Parse None 
        |> Map.toList
        |> should be (equalto [
        ("array__0__keyinside", "BOOM")
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
                |> Parse (Some "PREFIX")
                |> Print
                |> should equalto "PREFIX_array__0__keyinside=BOOM\n\
                                   PREFIX_key1=value1\n\
                                   PREFIX_key2=value2\n\
                                   PREFIX_key3=False\n\
                                   PREFIX_key4=2012-04-23T18:25:43.511Z\n\
                                   PREFIX_key5=\n\
                                   PREFIX_key6=5\n\
                                   PREFIX_key7=1.33\n"