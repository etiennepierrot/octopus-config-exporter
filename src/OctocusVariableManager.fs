module OctocusVariableManager

type ScopedKey = { Key :string; Scope :Option<string> }

type Change =
    {
        OldValue :Option<string>
        NewValue :string
    }

let Plan projectName (environnmentVariables :Map<string, string>) (scope :Option<string>) (getVariableSet :string ->  Map<ScopedKey, string>) =
    let variableSet = getVariableSet projectName
    let filterVariableSet key value =
        not (variableSet.ContainsKey({Key = key; Scope = scope;}))  || 
        (variableSet[{Key = key; Scope = scope;}] <> value)
    let createChange scopedKey value = 
        {
            OldValue = variableSet.TryFind scopedKey
            NewValue = value
        }
    environnmentVariables 
            |> Map.filter filterVariableSet
            |> Map.map (fun k v -> createChange {Key = k; Scope = scope;}  v)

let Apply (changes :Map<ScopedKey, Change>) projectName (updateVariableSet :Map<ScopedKey, string> -> unit) =
     changes
     |> Map.map (fun k v -> v.NewValue )
     |> updateVariableSet

let UpdateProjectEnvironnmentVariable  projectName (environnmentVariables : Map<string, string>) (scope :Option<string>) (updateVariableSet :Map<ScopedKey, string> -> unit)  getVariableSet =
    let changes = Plan projectName environnmentVariables scope getVariableSet
                    |> Map.toList 
                    |> List.map ( fun (k, v) -> ({Key = k; Scope = scope;}, v) ) 
                    |> Map.ofList
    Apply changes projectName updateVariableSet

let GetProjectEnvironnmentVariables projectName (getVariableSet : string ->  Map<ScopedKey, string>) =
    getVariableSet projectName
    |> Seq.toList 
    |> Seq.map( fun var -> (var.Key, var.Value))