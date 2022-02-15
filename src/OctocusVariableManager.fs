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
    


let Plan (scope :Option<string>) (getVariableSet :unit ->  Map<ScopedKey, string>) (environnmentVariables :Map<string, string>) =
    let variableSet = getVariableSet()
    let scopedKey key = {Key = key; Scope = scope;}
    let filterVariableSet key value =
        not (variableSet.ContainsKey(scopedKey key))  || 
        variableSet[scopedKey key] <> value
    let createChange key value = 
        match variableSet.TryFind (scopedKey key) with
        | Some oldValue -> 
            {
                OldValue = oldValue
                NewValue = value
            } |> Modify
        | None -> value |> New
    environnmentVariables 
    |> Map.filter filterVariableSet
    |> Map.map createChange
    |> Seq.map( fun var -> ((scopedKey var.Key), var.Value))
    |> Map.ofSeq

let Apply (updateVariableSet :Map<ScopedKey, string> -> unit) (changes :Map<ScopedKey, Change>) =
    let updatedValue = function 
                        | Modify s -> s.NewValue 
                        | New s -> s
    changes
    |> Map.map (fun _ v -> updatedValue v )
    |> updateVariableSet


let GetProjectEnvironnmentVariables (getVariableSet : unit ->  Map<ScopedKey, string>) =
    getVariableSet()
    |> Seq.toList 
    |> Seq.map( fun var -> (var.Key, var.Value))