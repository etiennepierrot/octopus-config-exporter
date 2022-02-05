module CompositionRoot
open Argu
open CliAgurmentParser
open OctocusVariableManager
open VarJsonParser

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
    let updateOctopusVariables = OctopusConnector.UpdateVariableSet octopusServer apiKey
    let getOctopusVariables = OctopusConnector.GetVariableSet octopusServer apiKey
    let environnmentVariables = pathConfigFile |> readfile |> Parse prefix 
    UpdateProjectEnvironnmentVariable projectName environnmentVariables updateOctopusVariables getOctopusVariables |> ignore