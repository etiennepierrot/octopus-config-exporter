module OctopusConnector
open Octopus.Client
open Octopus.Client.Model
open OctocusVariableManager

type OctopusConfig = { Url :string; ApiKey :string; ProjectName :string; }

type OctopusWrapper(octopusConfig :OctopusConfig) =

    let findEnvironnment = Helper.Memoize (fun (repo :OctopusRepository )->  repo.Environments.FindAll() |> Seq.toList )
    let mutable octopusConfig = octopusConfig

    let repo = OctopusServerEndpoint(octopusConfig.Url, octopusConfig.ApiKey) |> OctopusRepository

    member private this.getVariableSet projectName = 
        let projectResource = repo.Projects.FindByName projectName
        repo.VariableSets.Get projectResource.VariableSetId 

    member private this.getOctopusScopeSpecification (scope :string) =
        let scopeValue =  (repo 
                            |> findEnvironnment
                            |> List.find(fun e -> e.Name = scope)).Id
                            |> ScopeValue
        let scope = new ScopeSpecification()
        scope.Add( ScopeField.Environment, scopeValue ) 
        scope

    member private this.getScopeByEnvironnmentId (environnementId :string) =
        (repo 
        |> findEnvironnment
        |> List.find(fun (e :EnvironmentResource) ->  e.Id = environnementId)).Name


    member this.GetVariableSet = 
        let convertToMap (variableResource :VariableResource) :  Map<ScopedKey, string> =
            if variableResource.Scope.Count = 0 then
                Map [{Key = variableResource.Name; Scope  = None}, variableResource.Value ;  ]
            else
                variableResource.Scope[ScopeField.Environment]
                |> Seq.map(fun x -> {Key = variableResource.Name; Scope  = Some (this.getScopeByEnvironnmentId x)}, variableResource.Value ;)
                |> Map.ofSeq
        (this.getVariableSet octopusConfig.ProjectName).Variables
                |> Seq.map convertToMap
                |> Seq.fold Helper.Join Map.empty
                
    member this.UpdateVariableSet (environnmentVariables :Map<ScopedKey, string>) =
        let variableSet = this.getVariableSet octopusConfig.ProjectName
        let addOrUpdateVariableValue (key :ScopedKey) (value :string) =
            match key.Scope with
            | Some s -> variableSet.AddOrUpdateVariableValue(key.Key, value, this.getOctopusScopeSpecification s) 
            | None -> variableSet.AddOrUpdateVariableValue(key.Key, value)
        environnmentVariables |> Seq.iter(fun env -> addOrUpdateVariableValue env.Key env.Value |> ignore) 
        repo.VariableSets.Modify(variableSet) |> ignore

    member this.CreateProject octopusConfig =
        let projectResource = ProjectResource()
        projectResource.Name <- octopusConfig.ProjectName
        projectResource.ProjectGroupId <- "ProjectGroups-1"
        projectResource.LifecycleId <- "Default Lifecycle"
        projectResource.IsDisabled <- false
        repo.Projects.Create projectResource |> ignore
