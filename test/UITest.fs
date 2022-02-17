module UITest
open UI
open OctocusVariableManager
open Xunit
open Xunit.Abstractions
open FsUnit
open FsUnit.Xunit

let equalto = equal

type``Display should``(output:ITestOutputHelper) =

    [<Theory>]
    [<InlineData("password_super_secure")>]
    [<InlineData("credential")>]
    let ``hide sensitive data`` (keyName) =
        Map[
            { Key = "fizz"; Scope  = None}, Modify { OldValue = "old_buzz"; NewValue = "new_buzz"};
            { Key = keyName; Scope  = None}, Modify { OldValue = "HYPERSENTIVE DATA"; NewValue = "ANOTHER HYPERSENTIVE DATA"};
        ]
        |> RedactSensitiveData
        |> DisplayPlan
        |> Seq.fold (+) ""
        |> (fun s ->  not (s.Contains("HYPERSENTIVE DATA")) || not (s.Contains("ANOTHER HYPERSENTIVE DATA")) )
        |> should equalto true
        