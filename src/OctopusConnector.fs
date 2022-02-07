module OctopusConnector
open Octopus
open Octopus.Client
open Octopus.Client.Model
open OctocusVariableManager
open System.Collections.Generic

let getOctopusRepository serverUrl apikey = OctopusServerEndpoint(serverUrl, apikey) |> OctopusRepository

let memoize f =
    let dict = Dictionary<_, _>();
    fun c ->
        let exist, value = dict.TryGetValue c
        match exist with
        | true -> value
        | _ -> 
            let value = f c
            dict.Add(c, value)
            value

let findEnvironnment (repo :OctopusRepository) = 
    repo.Environments.FindAll()

let memoFindEnvironnment = memoize findEnvironnment

let getVariableSet (repo :OctopusRepository) = repo.Projects.FindByName >> (fun p -> repo.VariableSets.Get(p.VariableSetId) ) 
let getEnvironnmentByScope (repo :OctopusRepository) (scope :string) =
     memoFindEnvironnment repo
     |> Seq.toList 
     |> List.find(fun e -> e.Name = scope)

let getScopeByEnvironnmentId (repo :OctopusRepository) (environnementId :string) =
    (memoFindEnvironnment repo
    |> Seq.toList 
    |> List.find(fun e -> e.Id = environnementId)).Name

let GetVariableSet octopusServer apiKey projectName = 
    let repo = getOctopusRepository octopusServer apiKey
    let join (p:Map<'a,'b>) (q:Map<'a,'b>) = 
        Map(Seq.concat [ (Map.toSeq p) ; (Map.toSeq q) ])
    let convertToMap (variableResource :VariableResource) :  Map<ScopedKey, string> =
        if variableResource.Scope.Count = 0 then
            Map [{Key = variableResource.Name; Scope  = None}, variableResource.Value ;  ]
        else
            variableResource.Scope[ScopeField.Environment]
            |> Seq.map(fun x -> getScopeByEnvironnmentId repo x )
            |> Seq.map(fun x -> {Key = variableResource.Name; Scope  = Some x}, variableResource.Value ;)
            |> Map.ofSeq

    (getVariableSet repo projectName).Variables
    |> Seq.map convertToMap
    |> Seq.fold join Map.empty

let UpdateVariableSet octopusServer apiKey projectName (environnmentVariables :Map<ScopedKey, string>) =
    let repo = getOctopusRepository octopusServer apiKey
    let variableSet = getVariableSet repo projectName
    let scope (s :string) = 
        let scope = new ScopeSpecification()
        let env = getEnvironnmentByScope repo s
        scope.Add( ScopeField.Environment, new ScopeValue(env.Id) ) 
        scope
    let addOrUpdateVariableValue (key :ScopedKey) (value :string) =
        match key.Scope with
        | Some s -> variableSet.AddOrUpdateVariableValue(key.Key, value, scope s) 
        | None -> variableSet.AddOrUpdateVariableValue(key.Key, value)
    environnmentVariables |> Seq.iter(fun env -> addOrUpdateVariableValue env.Key env.Value |> ignore) 
    repo.VariableSets.Modify(variableSet) |> ignore