module CliAgurmentParser
open Argu
open OctocusVariableManager
open System


type CliArguments =
    | [<Mandatory>] [<MainCommand; ExactlyOnce; First>] ConfigFile of path:string
    | [<Mandatory>] [<CustomAppSettings "OCTOPUS_SERVER">] OctopusServer of server:string
    | [<Mandatory>] [<CustomAppSettings "OCTOPUS_API_KEY">] OctopusApiKey of apiKey:string
    | [<Mandatory>] OctopusProject of projectName:string
    | Prefix of prefix:string
    | Scope of prefix:string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | ConfigFile _ -> "specify a json config file to export"
            | OctopusServer _ -> "specify a the address of the octopus server"
            | OctopusApiKey _ -> "specify your api key"
            | OctopusProject _ -> "specify the project name"
            | Prefix _ -> "specify the prefix of Env Var"
            | Scope _ -> "scope for applying config"


let DisplayPlan plan= 
    let display (key :ScopedKey)  (change :Change) = 
        match key.Scope with
        | Some scope ->  match change with 
                                        | Modify mv -> sprintf "Modified variable %s from %s to %s in %s environnement" key.Key mv.OldValue mv.NewValue scope
                                        | New s -> sprintf "New variable %s with %s in %s environnement" key.Key s scope
        | None ->  match change with 
                                        | Modify mv -> sprintf "Modified variable %s from %s to %s" key.Key mv.OldValue mv.NewValue
                                        | New s -> sprintf "New variable %s with %s" key.Key s

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
