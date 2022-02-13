module CompositionRoot
open CliAgurmentParser
open VarJsonParser
open OctopusConnector

let Run (octopusConfig :OctopusConfig) (pathConfigFile :string) (prefix :option<string>) (scope :option<string>) = 

  let updateOctopusVariables = OctopusConnector.UpdateVariableSet octopusConfig
  let getOctopusVariables _ =  OctopusConnector.GetVariableSet octopusConfig
    
  let Plan = OctocusVariableManager.Plan scope getOctopusVariables
  let Apply = OctocusVariableManager.Apply updateOctopusVariables
  let ExtractChangesPlanFromFile = System.IO.File.ReadAllText >> (Parse prefix) >> Plan 
  
  let plan = pathConfigFile |> ExtractChangesPlanFromFile
  DisplayPlan plan
  AskApply (Apply) (plan)


