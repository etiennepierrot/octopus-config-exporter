open System
open VarJsonParser

[<EntryPoint>]
let main(args) =
    let readfile = System.IO.File.ReadAllText
    //printfn "env.cmdline: %A" <| Environment.GetCommandLineArgs()
    let path = args[0]
    let prefix =  if(args.Length > 1) then args[1] else ""
    path |> readfile |> Parse prefix |> Print |> printfn "%s"
    0