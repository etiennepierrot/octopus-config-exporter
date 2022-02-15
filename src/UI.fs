module UI
open OctocusVariableManager
open System

let DisplayPlan plan = 
    let display (key :ScopedKey)  (change :Change) = 
        match key.Scope with
        | Some scope ->  match change with 
                                        | Modify mv -> sprintf "Modified variable %s from %s to %s in %s environnement" 
                                                                key.Key mv.OldValue mv.NewValue scope
                                        | New s -> sprintf "New variable %s with %s in %s environnement" 
                                                            key.Key s scope
        | None ->  match change with 
                                        | Modify mv -> sprintf "Modified variable %s from %s to %s" 
                                                                key.Key mv.OldValue mv.NewValue
                                        | New s -> sprintf "New variable %s with %s" 
                                                            key.Key s

    plan 
    |> Map.map display 
    |> Map.iter (fun k v -> printfn "%s" v)


let rec AskApply (apply : (Map<ScopedKey, Change>) -> unit) (plan :Map<ScopedKey, Change>) = 
    if plan.IsEmpty then
        printfn "No changes to apply"
        0
    else  
        printfn "Do you want to apply theses changes? (Y/n)"
        let answer  = Console.ReadKey()
        printfn ""  
        match answer.KeyChar with
        | 'Y' ->  
            apply plan
            0
        | 'n' -> 0
        | _   -> AskApply apply plan
