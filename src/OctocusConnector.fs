module OctocusConnector
open Octopus


let GetOctopusRepository serverUrl apikey = Client.OctopusServerEndpoint(serverUrl, apikey) |>  Client.OctopusRepository

let UpdateProjectEnvironnmentVariable  projectName (repo :Client.OctopusRepository) (environnmentVariables : (string * string)list) =
    let project = repo.Projects.FindByName(projectName);
    let variableSet = repo.VariableSets.Get(project.VariableSetId)
    let set ((key, value) : (string * string)) = 
        variableSet.AddOrUpdateVariableValue(key, value)  |> ignore
    environnmentVariables |> Seq.iter set
    repo.VariableSets.Modify(variableSet) |> ignore


let GetProjectEnvironnmentVariables projectName (repo :Client.OctopusRepository) =
    let project = repo.Projects.FindByName(projectName);
    let variableSet = repo.VariableSets.Get(project.VariableSetId)
    variableSet.Variables 
    |> Seq.toList 
    |> Seq.map( fun var -> (var.Name, var.Value))
