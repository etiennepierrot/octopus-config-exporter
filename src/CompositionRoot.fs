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
        Url = parsedResult.GetResult OctopusServer;
        ApiKey = parsedResult.GetResult OctopusApiKey; 
      }

  let updateOctopusVariables = OctopusConnector.UpdateVariableSet octopusConfig projectName
  let getOctopusVariables = OctopusConnector.GetVariableSet octopusConfig
  let ParseEnvironnmentVariablesFromFile = System.IO.File.ReadAllText >> (Parse prefix)
  let environnmentVariables = ParseEnvironnmentVariablesFromFile pathConfigFile

  let plan = Plan projectName environnmentVariables scope getOctopusVariables
  let ApplyPlan plan = Apply plan projectName updateOctopusVariables
  
  DisplayPlan plan
  AskApply (ApplyPlan) (plan)


