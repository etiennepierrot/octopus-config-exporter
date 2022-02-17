[<EntryPoint>]
let main(args) = 
    CompositionRoot.Run 
    |> CliAgurmentParser.ParseArgs 
