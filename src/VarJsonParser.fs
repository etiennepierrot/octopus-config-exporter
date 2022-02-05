module VarJsonParser
open FSharp.Data

let Print (varsEnv: Map<string,  string>) : string = varsEnv   
                                                    |> Map.toList
                                                    |> List.map(fun (key, value) -> key + "=" + value + "\n")
                                                    |> List.fold (+) ""

let Parse (prefix :string) (json :string) : Map<string, string> =
    let join (p:Map<'a,'b>) (q:Map<'a,'b>) = 
        Map(Seq.concat [ (Map.toSeq p) ; (Map.toSeq q) ])


    let rec parseNode (key :string) (acc :Map<string, string>) (jsonValue :JsonValue) : Map<string, string> = 
        let accumulate value = acc.Add(key, value)
        let parseChild childKey node = parseNode childKey acc node
        let concat (motherKey :string) (childKey :string) = if motherKey = "" then childKey else (motherKey + "_" + childKey)

        let exploreRecord record  = record 
                                    |> Array.map(fun (childKey, child) -> child |> parseChild ( concat key  childKey) ) 
                                    |> Array.fold join Map.empty
        let exploreArray array = array 
                                    |> Array.mapi(fun idx child -> child |> parseChild ( concat key  (idx |> string)) ) 
                                    |> Array.fold join Map.empty

        match jsonValue with
        | JsonValue.String  s -> s  |> accumulate 
        | JsonValue.Boolean b -> b  |> string |> accumulate
        | JsonValue.Null    _ -> "" |> accumulate
        | JsonValue.Number  n -> n  |> string |> accumulate
        | JsonValue.Record  r -> r  |> exploreRecord
        | JsonValue.Array   a -> a  |> exploreArray
        | _                   -> raise (System.ArgumentException("Parsing error"))
    JsonValue.Parse(json) |> parseNode prefix Map.empty

