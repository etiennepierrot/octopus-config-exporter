module CompositionRoot
open Argu
open CliAgurmentParser
open OctocusVariableManager
open VarJsonParser
open OctopusConnector

let Run (parsedResult :ParseResults<CliArguments>) = 
  let pathConfigFile = parsedResult.GetResult ConfigFile
  let projectName = parsedResult.GetResult OctopusProject
  let prefix = parsedResult.TryGetResult Prefix
  let scope = parsedResult.TryGetResult Scope
    
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


