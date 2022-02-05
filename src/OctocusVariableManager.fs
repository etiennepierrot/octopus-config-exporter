module OctocusVariableManager

type OctopusVariable = { Value :string; Scope :Option<string> }

type Change =
    {
        OldValue :Option<string>
        NewValue :string
    }

let Plan projectName (environnmentVariables : Map<string, string>) (getVariableSet : string ->  Map<string, OctopusVariable list>) =
    let variableSet = getVariableSet projectName
    environnmentVariables 
    |> Map.filter( fun k v -> not (variableSet.ContainsKey(k)) ||  variableSet[k].Head.Value <> v ) 
    |> Map.map(fun k v -> { OldValue = ( if variableSet.ContainsKey(k) then Some variableSet[k].Head.Value else None); NewValue = v } )

let Apply (changes :Map<string, Change>) projectName  updateVariableSet=
    changes
    |> Map.map (fun _ v -> [{Value = v.NewValue; Scope = None}] )
    |> updateVariableSet projectName
    |> ignore

let UpdateProjectEnvironnmentVariable  projectName (environnmentVariables : Map<string, string>) updateVariableSet getVariableSet =
    let changes = Plan projectName environnmentVariables getVariableSet
    Apply changes projectName updateVariableSet |> ignore

let GetProjectEnvironnmentVariables projectName (getVariableSet : string ->  Map<string, OctopusVariable list>) =
    let variableSet = getVariableSet projectName
    variableSet 
    |> Seq.toList 
    |> Seq.map( fun var -> (var.Key, var.Value))


    