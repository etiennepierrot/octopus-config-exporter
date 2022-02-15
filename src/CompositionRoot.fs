module CompositionRoot
open VarJsonParser
open OctopusConnector
open UI

let Run (octopusConfig :OctopusConfig) (pathConfigFile :string) (prefix :option<string>) (scope :option<string>) = 

  let octopusWrapper = new OctopusWrapper(octopusConfig)
  let updateOctopusVariables = octopusWrapper.UpdateVariableSet
  let getOctopusVariables _ =  octopusWrapper.GetVariableSet
    
  let Plan = OctocusVariableManager.Plan scope getOctopusVariables
  let Apply = OctocusVariableManager.Apply updateOctopusVariables
  let ExtractChangesPlanFromFile = System.IO.File.ReadAllText >> (Parse prefix) >> Plan 
  
  let plan = pathConfigFile |> ExtractChangesPlanFromFile
  DisplayPlan plan
  AskApply (Apply) (plan)


