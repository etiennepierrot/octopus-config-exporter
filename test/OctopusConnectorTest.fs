module OctopusConnectorTest
open FsUnit
open Xunit
open Xunit.Abstractions
open CliAgurmentParser
open Argu
open OctopusConnector
open OctocusVariableManager
open Helper
open System


type TestCliArguments =
    | [<Mandatory>] [<CustomAppSettings(OctopusServerAppSettings)>] OctopusServer of server:string
    | [<Mandatory>] [<CustomAppSettings(OctopusApiKeyAppSettings)>] OctopusApiKey of apiKey:string
    | [<Mandatory>] [<CustomAppSettings(OctopusProjectNameAppSettings)>] OctopusProject of projectName:string

    interface IArgParserTemplate with
        member s.Usage = ""

type ``Connector``(output:ITestOutputHelper) =

    let GetOctopusConfig (parsedResult :ParseResults<TestCliArguments>) = 
                { 
                    Url = parsedResult.GetResult OctopusServer;
                    ApiKey = parsedResult.GetResult OctopusApiKey; 
                    ProjectName = parsedResult.GetResult OctopusProject
                }

    let octopusConfig = 
        let originalConfig = ParseResult<TestCliArguments>(true) |> GetOctopusConfig
        {originalConfig with ProjectName = Guid.NewGuid().ToString().Substring(0, 8)} 

    let UpdateVariableSet = OctopusConnector.UpdateVariableSet octopusConfig
    do 
        CreateProject octopusConfig |> ignore


    [<Fact>]
    let ``GetVariables should retrieve variables from octopus`` () =
        UpdateVariableSet Map[ { Key = "newKey"; Scope  = None}, "NewValue";] 
        GetVariableSet octopusConfig
        |> should be (equal 
            Map[
                ({ Key = "newKey"; Scope = None }, "NewValue"); 
            ])
    [<Fact>]
    let ``GetVariables should retrieve variables from octopus with scope`` () =
        UpdateVariableSet Map[ { Key = "newKey"; Scope  = QA}, "NewValue";] 
        GetVariableSet octopusConfig
        |> should be (equal 
            Map[
                ({ Key = "newKey"; Scope = QA }, "NewValue"); 
            ])

    [<Fact>]
    let ``UpdateVariable should modify octopus variable`` () =
        UpdateVariableSet Map[ { Key = "newKey"; Scope  = None}, "NewValue";] 
        UpdateVariableSet Map[ { Key = "newKey"; Scope  = None}, "AnotherNewValue";] 
        GetVariableSet octopusConfig
        |> should be (equal 
            Map[
                ({ Key = "newKey"; Scope = None }, "AnotherNewValue"); 
            ])




