open VarJsonParser
open OctocusConnector
open Argu


// --configfile config_sample.json --octopusserver https://etienne.octopus.app/ --octopusapikey *** --octopusproject test --prefix PREFIX
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

[<EntryPoint>]
let main(args) =
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
    let repo = GetOctopusRepository octopusServer apiKey

    
    let t = pathConfigFile |> readfile |> Parse prefix 
    UpdateProjectEnvironnmentVariable projectName repo t  |> ignore
    0