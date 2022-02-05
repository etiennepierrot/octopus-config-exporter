module OctocusConnector
open Octopus


let UpdateProjectEnvironnmentVariable  projectName (environnmentVariables : Map<string, string>) getVariableSet updateVariableSet =
    let set (key, value) (variableSet :Client.Model.VariableSetResource )= 
        variableSet.AddOrUpdateVariableValue(key, value)  |> ignore
    let variableSet = getVariableSet projectName
    environnmentVariables |> Seq.iter (fun ev -> set (ev.Key, ev.Value) variableSet)
    updateVariableSet variableSet


//let Plan projectName (environnmentVariables : (string * string)) getVariableSet =
//    let octopusEnvVar = GetProjectEnvironnmentVariables projectName getVariableSet



let GetProjectEnvironnmentVariables projectName (getVariableSet : string ->  Client.Model.VariableSetResource) =
    let variableSet = getVariableSet projectName
    variableSet.Variables 
    |> Seq.toList 
    |> Seq.map( fun var -> (var.Name, var.Value))

    