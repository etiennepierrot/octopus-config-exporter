module VarJsonParser
open FSharp.Data

type Node =
        | Key of string
        | Root of Option<string>

let Print (varsEnv: Map<string,  string>) : string = 
    varsEnv   
    |> Map.toList
    |> List.map(fun (key, value) -> key + "=" + value + "\n")
    |> List.fold (+) ""

let Parse (prefix :Option<string>) (json :string) : Map<string, string> =
    let join (p:Map<'a,'b>) (q:Map<'a,'b>) = 
        Map(Seq.concat [ (Map.toSeq p) ; (Map.toSeq q) ])

    let rec parseNode (key :Node) (map :Map<string, string>) (jsonValue :JsonValue) : Map<string, string> = 
        
        let parseChild (childKey :Node) node = parseNode childKey map node
        let concat (childKey :string) = 
            match key with
            | Key k ->  k + "__" + childKey |> Key
            | Root(Some prefix) ->  (prefix + "_" + childKey) |> Key
            | Root (None)  ->  childKey |> Key

        let exploreRecord record  = record 
                                    |> Array.map(fun (childKey, child) -> child |> parseChild ( concat childKey) ) 
                                    |> Array.fold join Map.empty
        let exploreArray array = array 
                                    |> Array.mapi(fun idx child -> child |> parseChild ( concat  (idx |> string ) ) ) 
                                    |> Array.fold join Map.empty

        match key with 
        | Root _ ->  match jsonValue with
                        | JsonValue.Record  r -> r  |> exploreRecord 
                        | _                   -> raise (System.ArgumentException("Parsing error"))
        |Key node -> match jsonValue with
                        | JsonValue.String  s -> map.Add ( node, s )
                        | JsonValue.Boolean b -> map.Add ( node, b  |> string) 
                        | JsonValue.Null    _ -> map.Add ( node, "" )
                        | JsonValue.Number  n -> map.Add ( node, n  |> string )
                        | JsonValue.Record  r -> r  |> exploreRecord
                        | JsonValue.Array   a -> a  |> exploreArray
                        | _                   -> raise (System.ArgumentException("Parsing error"))

    JsonValue.Parse(json) |> parseNode (prefix |> Root) Map.empty

