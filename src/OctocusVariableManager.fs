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

let Apply (updateVariableSet :Map<ScopedKey, string> -> unit) (changes :Map<ScopedKey, Change>) =
    let updatedValue = function 
                        | Modify s -> s.NewValue 
                        | New s -> s
    changes
    |> Map.map (fun k v -> updatedValue v )
    |> updateVariableSet


let GetProjectEnvironnmentVariables (getVariableSet : unit ->  Map<ScopedKey, string>) =
    getVariableSet()
    |> Seq.toList 
    |> Seq.map( fun var -> (var.Key, var.Value))