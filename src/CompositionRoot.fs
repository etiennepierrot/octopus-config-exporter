module CompositionRoot
open Argu
open OctocusConnector
open VarJsonParser
open Octopus

type CliArguments =
    | ConfigFile of path:string
    | OctopusServer of server:string
    | OctopusApiKey of apiKey:string
    | OctopusProject of projectName:string
    | Prefix of prefix:string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | ConfigFile _ -> "specify a json config file to export"
            | OctopusServer _ -> "specify a the address of the octopus server"
            | OctopusApiKey _ -> "specify your api key"
            | OctopusProject _ -> "specify the project name"
            | Prefix _ -> "specify the prefix of Env Var"

let Export = 
    let parser = ArgumentParser.Create<CliArguments>()
    let results = parser.Parse()
    let pathConfigFile = results.GetResult ConfigFile
    let octopusServer = results.GetResult OctopusServer
    let apiKey = results.GetResult OctopusApiKey
    let projectName = results.GetResult OctopusProject
    let prefix = match results.TryGetResult Prefix with
                 | Some prefix -> prefix
                 | _ -> ""
    
    let readfile = System.IO.File.ReadAllText
    let GetOctopusRepository serverUrl apikey = Client.OctopusServerEndpoint(serverUrl, apikey) |>  Client.OctopusRepository

    let repo = GetOctopusRepository octopusServer apiKey

    let getVariableSet = repo.Projects.FindByName >> (fun p -> repo.VariableSets.Get(p.VariableSetId) ) 

    let GetVariableSet = 
        let convertToMap ( varset :Client.Model.VariableSetResource) = 
            varset.Variables 
            |> Seq.map( fun var -> var.Name,  [{ Value = var.Value; Scope  = None }])
        getVariableSet >> convertToMap
    
    let UpdateVariableSet projectName  (environnmentVariables:Map<string, OctopusVariable list>)  =
        let variableSet = getVariableSet projectName
        let convertToVariableSet (environnmentVariables:Map<string, OctopusVariable list>) = 
            environnmentVariables |> Seq.iter(fun env -> variableSet.AddOrUpdateVariableValue(env.Key, env.Value.Head.Value) |> ignore)
            variableSet
        environnmentVariables |> convertToVariableSet |> repo.VariableSets.Modify |> ignore
    
    let environnmentVariables = pathConfigFile |> readfile |> Parse prefix 
    UpdateProjectEnvironnmentVariable projectName environnmentVariables UpdateVariableSet  |> ignore