module OctocusVariableManagerTest
open FsUnit
open OctocusVariableManager
open Xunit
open Xunit.Abstractions
open TestHelper

let equalto = equal


type``Plan should``(output:ITestOutputHelper) =

    let mutable MockOctopusVariables = Map[
                                        {Key = "fizz"; Scope  = None}, "buzz" ;  
                                        {Key = "fizz2"; Scope  = None}, "newbuzz";  
                                        {Key = "fizz2"; Scope  = QA}, "newbuzz-QA";  
                                        ]

    let GetVariableSet _ = MockOctopusVariables 
    let UpdateVariableSet (environnmentVariables :Map<ScopedKey, string>) = 
        let update key value = MockOctopusVariables <- (MockOctopusVariables |> Map.change key  (fun _ -> Some value) )    
        environnmentVariables |> Map.iter update
    let Plan = OctocusVariableManager.Plan None GetVariableSet
    
    [<Fact>]
    let ``indicate change on existing value`` () =
        Plan  (Map ["fizz", "buzz"; "fizz2", "CHANGED";]) 
        |> should be (equalto (Map[
        { Key = "fizz2"; Scope  = None}, Modify { OldValue = "newbuzz"; NewValue = "CHANGED"};
        ]))
    
    
    [<Fact>]
    let ``indicate change on new value`` () =
        Plan  ( Map ["something", "new"])
        |> should be (equalto (Map[
        { Key = "something"; Scope  = None}, New "new";
        ]))

type``Apply should``(output:ITestOutputHelper) =
    let mutable MockOctopusVariables = Map[
                                        {Key = "fizz"; Scope  = None}, "buzz" ;  
                                        {Key = "fizz2"; Scope  = None}, "newbuzz";  
                                        {Key = "fizz2"; Scope  = QA}, "newbuzz-QA";  
                                        ]

    let GetVariableSet _ = MockOctopusVariables 
    let UpdateVariableSet (environnmentVariables :Map<ScopedKey, string>) = 
        let update key value = MockOctopusVariables <- (MockOctopusVariables |> Map.change key  (fun _ -> Some value) )    
        environnmentVariables |> Map.iter update

    let getValue (key :ScopedKey) =  
        GetProjectEnvironnmentVariables GetVariableSet
        |> Seq.find(fun (k, _) -> k =  key) 
        |> snd 

    let Apply = OctocusVariableManager.Apply UpdateVariableSet

    [<Fact>]
    let ``modify octopus variable`` () =
        Map[ { Key = "fizz2"; Scope  = None}, New "anotherbuzz";]
        |> Apply
        getValue {Key =  "fizz2"; Scope = None; }
        |> should be (equalto "anotherbuzz")