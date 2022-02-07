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
    let prefix = results.TryGetResult Prefix
    
    let updateOctopusVariables = OctopusConnector.UpdateVariableSet octopusServer apiKey projectName
    let getOctopusVariables = OctopusConnector.GetVariableSet octopusServer apiKey
    let ParseEnvironnmentVariablesFromFile = System.IO.File.ReadAllText >> (Parse prefix)
    let environnmentVariables = ParseEnvironnmentVariablesFromFile pathConfigFile

    let plan = Plan projectName environnmentVariables None getOctopusVariables
    let ApplyPlan plan = Apply plan projectName updateOctopusVariables
    
    DisplayPlan plan
    AskApply (ApplyPlan) (plan)

