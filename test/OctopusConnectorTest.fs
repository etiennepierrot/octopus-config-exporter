module OctopusConnectorTest
open FsUnit
open VarJsonParser
open Xunit
open Xunit.Abstractions
open CliAgurmentParser
open Argu
open OctopusConnector
open OctocusVariableManager
open Helper

type``Connector``(output:ITestOutputHelper) =

    let write result = output.WriteLine
    let readfile = System.IO.File.ReadAllText
    let equalto = equal

type TestCliArguments =
    | [<Mandatory>] [<CustomAppSettings(OctopusServerAppSettings)>] OctopusServer of server:string
    | [<Mandatory>] [<CustomAppSettings(OctopusApiKeyAppSettings)>] OctopusApiKey of apiKey:string
    | [<Mandatory>] [<CustomAppSettings(OctopusProjectNameAppSettings)>] OctopusProject of projectName:string

    interface IArgParserTemplate with
        member s.Usage = ""
          
let GetOctopusConfig (parsedResult :ParseResults<TestCliArguments>) = 
    { 
        Url = parsedResult.GetResult OctopusServer;
        ApiKey = parsedResult.GetResult OctopusApiKey; 
        ProjectName = parsedResult.GetResult OctopusProject
    }



[<Fact>]
let ``GetVariables should retrieve varoiables from octopus`` () =
    ParseResult<TestCliArguments>(true) 
    |> GetOctopusConfig 
    |> OctopusConnector.GetVariableSet
    |> should be (equal 
        Map[
            ({ Key = "fizz"; Scope = QA }, "buzz"); 
            ({ Key = "fizz"; Scope = Prod }, "buzz"); 
            ({ Key = "PREFIX_array__0__keyinside"; Scope = None }, "BOOM");
            ({ Key = "PREFIX_key1"; Scope = None }, "value2"); 
            ({ Key = "PREFIX_key2"; Scope = None }, "value2"); 
            ({ Key = "PREFIX_key2"; Scope = Prod }, "value2"); 
            ({ Key = "PREFIX_key2"; Scope = QA }, "value2"); 
            ({ Key = "PREFIX_key3"; Scope = None }, "False"); 
            ({ Key = "PREFIX_key5"; Scope = None }, "");
            ({ Key = "PREFIX_key6"; Scope = None }, "5");
            ({ Key = "PREFIX_key7"; Scope = None }, "1.5");
        ])

