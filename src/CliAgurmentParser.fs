module CliAgurmentParser
open Argu
open OctocusVariableManager
open System
open DotNetConfig
open System.Collections.Generic
open OctopusConnector

[<Literal>]
let OctopusServerAppSettings = "OCTOPUS_SERVER"
[<Literal>]
let OctopusApiKeyAppSettings = "OCTOPUS_API_KEY"
[<Literal>]
let OctopusProjectNameAppSettings = "OCTOPUS_PROJECT_NAME"
[<Literal>]
let OctopusPrefix = "OCTOPUS_PREFIX"

let DotNetConfigToAppSettingsMapping =  Map [
            OctopusServerAppSettings, ("octopus", "serverUrl");
            OctopusApiKeyAppSettings, ("octopus", "apiKey");
            OctopusProjectNameAppSettings, ("octopus", "projectName");
            OctopusPrefix, ("octopus", "prefix");
        ] 

type CliArguments =
    | [<Mandatory>] [<MainCommand; ExactlyOnce; First>] ConfigFile of path:string
    | [<Mandatory>] [<CustomAppSettings(OctopusServerAppSettings)>] OctopusServer of server:string
    | [<Mandatory>] [<CustomAppSettings(OctopusApiKeyAppSettings)>] OctopusApiKey of apiKey:string
    | [<Mandatory>] [<CustomAppSettings(OctopusProjectNameAppSettings)>] OctopusProject of projectName:string
    | [<CustomAppSettings(OctopusPrefix)>] Prefix of prefix:string
    | Scope of prefix:string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | ConfigFile _ -> "specify a json config file to export"
            | OctopusServer _ ->  sprintf "specify a the address of the octopus server (can also be set with %s environnment variable)" OctopusServerAppSettings
            | OctopusApiKey _ -> sprintf "specify your api key (can also be set with %s environnment variable)" OctopusApiKeyAppSettings
            | OctopusProject _ -> sprintf "specify the project name (can also be set with %s environnment variable)" OctopusProjectNameAppSettings
            | Prefix _ -> sprintf "specify the prefix of Env Var (can also be set with %s environnment variable)" OctopusPrefix
            | Scope _ -> "scope for applying config"

let GetOctopusConfig (parsedResult :ParseResults<CliArguments>) = 
    { 
        Url = parsedResult.GetResult OctopusServer;
        ApiKey = parsedResult.GetResult OctopusApiKey; 
        ProjectName = parsedResult.GetResult OctopusProject
    }

let ParseResult<'a when 'a :> IArgParserTemplate> ignoreUnrecognized   = 
    let createDictionary =
        let dic = Dictionary<string, string>()
        let config = Config.Build()
        let addConfigFromDotnetConfig key (section, variable)  =
            let value = config.GetString(section, variable) 
            if(value <> null) then 
                dic.Add(key, value)
        DotNetConfigToAppSettingsMapping |> Map.iter addConfigFromDotnetConfig
        dic

    let dictionary = createDictionary
    let functionReader (key :string) : option<string> =
        if(dictionary.ContainsKey(key)) then 
            Some dictionary[key]
        else 
            Some (ConfigurationReader.FromEnvironmentVariables().GetValue(key))
    ArgumentParser.Create<'a>().Parse(
        configurationReader= (ConfigurationReader.FromFunction functionReader),
        ignoreUnrecognized = ignoreUnrecognized 

        )

let ParseArgs (exec : OctopusConfig -> string -> option<string> -> option<string> -> int )  =
    
    try
        let parsedResult = ParseResult(false)
        let octopusConfig = GetOctopusConfig parsedResult
        let pathConfigFile = parsedResult.GetResult ConfigFile
        let prefix = parsedResult.TryGetResult Prefix
        let scope = parsedResult.TryGetResult Scope
        exec octopusConfig pathConfigFile prefix scope
    with e ->
        printfn "%s" e.Message
        -1


let DisplayPlan plan= 
    let display (key :ScopedKey)  (change :Change) = 
        match key.Scope with
        | Some scope ->  match change with 
                                        | Modify mv -> sprintf "Modified variable %s from %s to %s in %s environnement" 
                                                                key.Key mv.OldValue mv.NewValue scope
                                        | New s -> sprintf "New variable %s with %s in %s environnement" 
                                                            key.Key s scope
        | None ->  match change with 
                                        | Modify mv -> sprintf "Modified variable %s from %s to %s" 
                                                                key.Key mv.OldValue mv.NewValue
                                        | New s -> sprintf "New variable %s with %s" 
                                                            key.Key s

    plan 
    |> Map.map display 
    |> Map.iter (fun k v -> printfn "%s" v)


let rec AskApply (apply : 'a -> unit) plan = 
    printfn "Do you want to apply theses changes? (Y/n)"
    let answer  = Console.ReadKey()
    printfn ""  
    match answer.KeyChar with
    | 'Y' ->  
        apply plan
        0
    | 'n' -> 0
    | _   -> AskApply apply plan
