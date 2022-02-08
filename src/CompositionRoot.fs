module CompositionRoot
open Argu
open CliAgurmentParser
open OctocusVariableManager
open VarJsonParser
open OctopusConnector

let Export = 
    let parser = ArgumentParser.Create<CliArguments>()
    let results = parser.Parse()
    let pathConfigFile = results.GetResult ConfigFile
    let prefix = results.TryGetResult Prefix
    let scope = results.TryGetResult Scope
    
    let octopusConfig = 
        { 
          Url = results.GetResult OctopusServer;
          ApiKey = results.GetResult OctopusApiKey; 
          ProjectName = results.GetResult OctopusProject
        }
    let updateOctopusVariables = OctopusConnector.UpdateVariableSet octopusConfig
    let getOctopusVariables _ =  OctopusConnector.GetVariableSet octopusConfig
    
    let Plan = OctocusVariableManager.Plan scope getOctopusVariables
    let Apply = OctocusVariableManager.Apply updateOctopusVariables
    let ExtractChangesPlanFromFile = System.IO.File.ReadAllText >> (Parse prefix) >> Plan 
   
    let plan = pathConfigFile |> ExtractChangesPlanFromFile
    DisplayPlan plan
    AskApply (Apply) (plan)

