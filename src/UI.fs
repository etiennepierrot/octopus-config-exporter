module UI
open OctocusVariableManager
open System

let DisplayPlan plan = 
    let display (scopedKey :ScopedKey)  (change :Change) = 
        let displayValue (value :Value) =
            let isSensitive = ["password"; "credential"]  
                                |> List.map scopedKey.Key.Contains
                                |> List.reduce (||)
            if isSensitive
            then 
                "**********"
            else
                sprintf "%s" value

        let printNewVariable newVariable :Value = sprintf "New variable %s with %s" scopedKey.Key (newVariable |> displayValue)
        let printModifiedVariable (modifiedVariable :ModifyVariable) = sprintf "Modified variable %s from %s to %s"
                                                                        scopedKey.Key 
                                                                        (modifiedVariable.OldValue |> displayValue) 
                                                                        (modifiedVariable.NewValue |> displayValue)
        
        match (scopedKey.Scope, change ) with
        | (Some scope, New n) ->  sprintf "%s in %s environnement" (printNewVariable n) scope
        | (None, New n)       ->  printNewVariable n
        | (None, Modify modify )        -> printModifiedVariable modify
        | (Some scope, Modify modify )  ->  sprintf "%s in %s environnement" (printModifiedVariable modify) scope
    plan 
    |> Map.map display 
    |> Map.toSeq
    |> Seq.map (fun (_, v ) -> sprintf "%s" v  )

let rec AskApply (apply : (Map<ScopedKey, Change>) -> unit) (plan :Map<ScopedKey, Change>) = 
    if plan.IsEmpty then
        printfn "No changes to apply"
        0
    else  
        DisplayPlan plan
        |> Seq.iter (printfn "%s")
        printfn "Do you want to apply theses changes? (Y/n)"
        let answer  = Console.ReadKey()
        printfn ""  
        match answer.KeyChar with
        | 'Y' ->  
            apply plan
            0
        | 'n' -> 0
        | _   -> AskApply apply plan
