module VarJsonParser
open FSharp.Data

let Print varsEnv = varsEnv 
                    |> List.map(fun (key, value) -> key + "=" + value + "\n")
                    |> List.fold (+) ""

let Parse (prefix :string) (json :string) =
    let rec parseNode (key :string) (acc) (jsonValue :JsonValue) = 
        let accumulate value = (key, value |> string)  :: acc
        let parseChild childKey node = parseNode childKey acc node

        let exploreRecord record  = record 
                                    |> Array.map(fun (childKey, child) -> child |> parseChild (key + "_" + childKey) ) 
                                    |> List.concat
        let exploreArray array = array 
                                    |> Array.mapi(fun idx child -> child  |> parseChild (key + "_" + (idx |> string)) ) 
                                    |> List.concat

        match jsonValue with
        | JsonValue.String  s -> s  |> accumulate 
        | JsonValue.Boolean b -> b  |> accumulate
        | JsonValue.Null    _ -> "" |> accumulate
        | JsonValue.Number  n -> n  |> accumulate
        | JsonValue.Record  r -> r  |> exploreRecord
        | JsonValue.Array   a -> a  |> exploreArray
        | _                   -> raise (System.ArgumentException("Parsing error"))
    JsonValue.Parse(json) |> parseNode prefix List.empty

