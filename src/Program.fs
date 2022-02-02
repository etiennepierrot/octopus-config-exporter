open System
open VarJsonParser

[<EntryPoint>]
let main(args) =
    let readfile = System.IO.File.ReadAllText
    //printfn "env.cmdline: %A" <| Environment.GetCommandLineArgs()
    let path = args[0]
    let prefix = args[1]
    path |> readfile |> Parse prefix |> Print |> printfn "%s"
    0