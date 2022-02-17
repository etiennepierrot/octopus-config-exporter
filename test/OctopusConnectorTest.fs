module OctopusConnectorTest
open FsUnit
open Xunit
open Xunit.Abstractions
open CliAgurmentParser
open Argu
open OctopusConnector
open OctocusVariableManager
open TestHelper
open System

type TestCliArguments =
    | [<Mandatory>] [<CustomAppSettings(OctopusServerAppSettings)>] OctopusServer of server:string
    | [<Mandatory>] [<CustomAppSettings(OctopusApiKeyAppSettings)>] OctopusApiKey of apiKey:string
    | [<Mandatory>] [<CustomAppSettings(OctopusProjectNameAppSettings)>] OctopusProject of projectName:string

    interface IArgParserTemplate with
        member s.Usage = ""

[<Trait("Category","Integration")>]
type ``Connector``(output:ITestOutputHelper) =

    let getOctopusConfig (parsedResult :ParseResults<TestCliArguments>) = 
                { 
                    Url = parsedResult.GetResult OctopusServer;
                    ApiKey = parsedResult.GetResult OctopusApiKey; 
                    ProjectName = parsedResult.GetResult OctopusProject
                }

    let originalConfig = ParseResult<TestCliArguments>(true) |> getOctopusConfig
    let testConfig = {originalConfig with ProjectName = Guid.NewGuid().ToString().Substring(0, 8)}
    
    let octopusWrapper = new OctopusWrapper(testConfig)
    do 
        octopusWrapper.CreateProject testConfig

    [<Fact>]
    let ``GetVariables should retrieve variables from octopus`` () =
        octopusWrapper.UpdateVariableSet Map[ { Key = "newKey"; Scope  = None}, "NewValue";] 
        octopusWrapper.GetVariableSet 
            |> should be (equal 
            Map[
                ({ Key = "newKey"; Scope = None }, "NewValue"); 
            ])
    [<Fact>]
    let ``GetVariables should retrieve variables from octopus with scope`` () =
        octopusWrapper.UpdateVariableSet Map[ { Key = "newKey"; Scope  = QA}, "NewValue";] 
        octopusWrapper.GetVariableSet 
        |> should be (equal 
            Map[
                ({ Key = "newKey"; Scope = QA }, "NewValue"); 
            ])

    [<Fact>]
    let ``UpdateVariable should modify octopus variable`` () =
        octopusWrapper.UpdateVariableSet Map[ { Key = "newKey"; Scope  = None}, "NewValue";] 
        octopusWrapper.UpdateVariableSet Map[ { Key = "newKey"; Scope  = None}, "AnotherNewValue";] 
        octopusWrapper.GetVariableSet 
        |> should be (equal 
            Map[
                ({ Key = "newKey"; Scope = None }, "AnotherNewValue"); 
            ])




