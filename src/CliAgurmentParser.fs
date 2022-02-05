module CliAgurmentParser
open Argu

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
