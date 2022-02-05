module OctopusConnector
open OctocusVariableManager
open Octopus

let getOctopusRepository serverUrl apikey = Client.OctopusServerEndpoint(serverUrl, apikey) |>  Client.OctopusRepository
let getVariableSet (repo :Client.OctopusRepository) = repo.Projects.FindByName >> (fun p -> repo.VariableSets.Get(p.VariableSetId) ) 

let GetVariableSet octopusServer apiKey projectName = 
    let repo = getOctopusRepository octopusServer apiKey
    let convertToMap ( varset :Client.Model.VariableSetResource) = 
        varset.Variables 
        |> Seq.map( fun var -> var.Name,  [{ Value = var.Value; Scope  = None }])
        |> Map.ofSeq
    getVariableSet repo projectName |> convertToMap

let UpdateVariableSet octopusServer apiKey projectName  (environnmentVariables:Map<string, OctopusVariable list>)  =
    let repo = getOctopusRepository octopusServer apiKey
    let variableSet = getVariableSet repo projectName
    let convertToVariableSet (environnmentVariables:Map<string, OctopusVariable list>) = 
        environnmentVariables |> Seq.iter(fun env -> variableSet.AddOrUpdateVariableValue(env.Key, env.Value.Head.Value) |> ignore)
        variableSet
    environnmentVariables |> convertToVariableSet |> repo.VariableSets.Modify |> ignore
