open System
#r @"../packages/FSharp.Compiler.Service/lib/net45/FSharp.Compiler.Service.dll"
open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.SourceCodeServices
open Microsoft.FSharp.Compiler.QuickParse

#r @"../packages/Newtonsoft.Json/lib/net45/Newtonsoft.Json.dll"
open Newtonsoft.Json

let checker = FSharpChecker.Create()

let parseWithTypeInfo (file, input) = 
    let checkOptions, _errors = checker.GetProjectOptionsFromScript(file, input) |> Async.RunSynchronously
    let parsingOptions, _errors = checker.GetParsingOptionsFromProjectOptions(checkOptions)
    let untypedRes = checker.ParseFile(file, input, parsingOptions) |> Async.RunSynchronously
    
    match checker.CheckFileInProject(untypedRes, file, 0, input, checkOptions) |> Async.RunSynchronously with 
    | FSharpCheckFileAnswer.Succeeded(res) -> untypedRes, res
    | res -> failwithf "Parsing did not finish... (%A)" res


let getDeclsFromVirturalFile (keyword:string) = 
    let virtualFile = "./virtual.fsx"
    let row = 1
    // col sample : "List." col 4, "" col 0
    let col = if String.IsNullOrEmpty keyword then 0 else keyword.Length - 1
    let partialName = QuickParse.GetPartialLongNameEx(keyword,col)
    let untyped, parsed = parseWithTypeInfo (virtualFile, keyword)

    parsed.GetDeclarationListInfo(Some untyped, row, keyword, partialName, (fun () -> [])) 
    |> Async.RunSynchronously


type JsonFormat = { word : string; info: string list list  }


// https://github.com/fsharp/FSharp.Compiler.Service/blob/master/src/fsharp/symbols/SymbolHelpers.fs#L232
let extractGroupTexts = function
    | FSharpToolTipElement.None -> []
    | FSharpToolTipElement.CompositionError s -> [s]
    | FSharpToolTipElement.Group (xs:FSharpToolTipElementData<string> list) -> xs |> List.map( fun (x:FSharpToolTipElementData<string>) -> x.MainDescription )


let body =
    [   ""
        "System"
        "Array";"List";"Seq";"Set";"Map"
        "Option";"Observable"
        "stdout";"stdin";"stderr"
    ]
    |> List.map ( fun (s:string) -> 
        let label = if String.IsNullOrEmpty s then "OneWord" else s
        let info  = getDeclsFromVirturalFile( if String.IsNullOrEmpty s then String.Empty else s + ".")
        let body  = info.Items
                    |> Array.fold ( fun state x ->
                        let dt : JsonFormat = { word = x.Name; info = match x.DescriptionText with FSharpToolTipText xs -> List.map extractGroupTexts xs }
                        state + " + \"\\n\" + " + "\"\"\"" +  JsonConvert.SerializeObject ( dt ) + "\"\"\"" ) ""
                    |> fun s -> s.Trim()
        String.replicate 12 " " + "\"" + label + "\"," + body.Substring(9)
        )
    |> List.reduce ( fun a b -> a + "\n" + b )

let head ="""
namespace dummyJson

module dummy =
    let dummy = 
        Map.ofSeq [
"""

let foot ="""
        ]
"""

head + body + foot |> stdout.WriteLine
