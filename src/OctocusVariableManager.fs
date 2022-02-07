module OctocusVariableManager

type ScopedKey = { Key :string; Scope :Option<string> }

type ModifyVariable = 
    {
       OldValue :string
       NewValue :string
    }

type Change =
    | Modify of ModifyVariable
    | New of string
   

let Plan projectName (environnmentVariables :Map<string, string>) (scope :Option<string>) (getVariableSet :string ->  Map<ScopedKey, string>) =
    let variableSet = getVariableSet projectName
    let filterVariableSet key value =
        not (variableSet.ContainsKey({Key = key; Scope = scope;}))  || 
        (variableSet[{Key = key; Scope = scope;}] <> value)

    let createChange scopedKey value = 
        match variableSet.TryFind scopedKey with
        | Some oldValue -> 
            {
                OldValue = oldValue
                NewValue = value
            } |> Modify
        | None -> value |> New
    environnmentVariables 
            |> Map.filter filterVariableSet
            |> Map.map (fun k v -> createChange {Key = k; Scope = scope;}  v)
            |> Seq.map( fun var -> ({Key = var.Key; Scope = scope;}, var.Value))
            |> Map.ofSeq

let Apply (changes :Map<ScopedKey, Change>) projectName (updateVariableSet :Map<ScopedKey, string> -> unit) =
    let updatedValue = function 
                        | Modify s -> s.NewValue 
                        | New s -> s
    changes
    |> Map.map (fun k v -> updatedValue v )
    |> updateVariableSet


let UpdateProjectEnvironnmentVariable  projectName (environnmentVariables : Map<string, string>) (scope :Option<string>) (updateVariableSet :Map<ScopedKey, string> -> unit)  getVariableSet =
    let changes = Plan projectName environnmentVariables scope getVariableSet
    Apply changes projectName updateVariableSet

let GetProjectEnvironnmentVariables projectName (getVariableSet : string ->  Map<ScopedKey, string>) =
    getVariableSet projectName
    |> Seq.toList 
    |> Seq.map( fun var -> (var.Key, var.Value))