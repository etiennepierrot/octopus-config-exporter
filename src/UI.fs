module UI
open OctocusVariableManager
open System

let RedactSensitiveData (map :Map<ScopedKey, Change>) : Map<ScopedKey, Change> = 
    let redactChange = function
                        | Modify _ -> { OldValue= "***REDACTED***"; NewValue = "***REDACTED***" } |> Modify
                        | New _ -> "***REDACTED***" |> New
    let redact (scopedKey :ScopedKey) (change :Change) = 
        let isSensitive = ["password"; "credential"]  
                                |> List.map scopedKey.Key.Contains
                                |> List.reduce (||)
        if isSensitive
        then 
            redactChange change
        else
            change
    map |> Map.map redact

let DisplayPlan plan = 
    let display (scopedKey :ScopedKey) (change :Change) = 
        let printNewVariable (newVariable :Value) = sprintf "New variable %s with %s" scopedKey.Key newVariable
        let printModifiedVariable (modifiedVariable :ModifyVariable) = sprintf "Modified variable %s from %s to %s"
                                                                        scopedKey.Key 
                                                                        modifiedVariable.OldValue 
                                                                        modifiedVariable.NewValue
    
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
