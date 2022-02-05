module OctocusVariableManager

type OctopusVariable = { Value :string; Scope :Option<string> }

let UpdateProjectEnvironnmentVariable  projectName (environnmentVariables : Map<string, string>) updateVariableSet =
    environnmentVariables 
    |> Map.map (fun _ v -> [{Value = v; Scope = None}] )
    |> updateVariableSet projectName


//let Plan projectName (environnmentVariables : (string * string)) getVariableSet =
//    let octopusEnvVar = GetProjectEnvironnmentVariables projectName getVariableSet


let GetProjectEnvironnmentVariables projectName (getVariableSet : string ->  Map<string, OctopusVariable list>) =
    let variableSet = getVariableSet projectName
    variableSet 
    |> Seq.toList 
    |> Seq.map( fun var -> (var.Key, var.Value))

    