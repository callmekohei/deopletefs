open System
#r @"../packages/FSharp.Compiler.Service/lib/net45/FSharp.Compiler.Service.dll"
open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.SourceCodeServices

#r @"../packages/Newtonsoft.Json/lib/net45/Newtonsoft.Json.dll"
open Newtonsoft.Json

let checker = FSharpChecker.Create()

let parseWithTypeInfo (file, input) = 
    let checkOptions, _errors = checker.GetProjectOptionsFromScript(file, input) |> Async.RunSynchronously
    let untypedRes = checker.ParseFileInProject(file, input, checkOptions) |> Async.RunSynchronously
    
    match checker.CheckFileInProject(untypedRes, file, 0, input, checkOptions) |> Async.RunSynchronously with 
    | FSharpCheckFileAnswer.Succeeded(res) -> untypedRes, res
    | res -> failwithf "Parsing did not finish... (%A)" res

let input = 
  """
stdin.
stdout.
  """
let inputLines = input.Split('\n')
let file = "./util/dummy.fsx"
System.IO.File.WriteAllText(file,input)

let untyped, parsed = parseWithTypeInfo (file, input)

let decls(keyword, col, row) = 
    parsed.GetDeclarationListInfo(Some untyped, col, row, "", keyword, "", (fun () -> [])) 
    |> Async.RunSynchronously

type JsonFormat = { word : string; info: string list list  }


// https://github.com/fsharp/FSharp.Compiler.Service/blob/master/src/fsharp/symbols/SymbolHelpers.fs#L232
let extractGroupTexts = function
    | FSharpToolTipElement.None -> []
    | FSharpToolTipElement.CompositionError s -> [s]
    | FSharpToolTipElement.Group (xs:FSharpToolTipElementData<string> list) -> xs |> List.map( fun (x:FSharpToolTipElementData<string>) -> x.MainDescription )


let body =
    [ []; ["System"] ; ["List"] ; ["Set"] ; ["Seq"] ; ["Array"] ; ["Map"] ; ["Option"] ; ["Observable"] ; ["Microsoft";"FSharp";"Core";"Operators";"stdout"] ; ["Microsoft";"FSharp";"Core";"Operators";"stdin"] ]
    |> List.map ( fun l -> 
                    let label = if List.isEmpty l then "OneWord" else List.last l
                    let info  = match label with
                                | "stdout" -> decls(l,3,7)
                                | "stdin"  -> decls(l,2,6)
                                | _        -> decls(l,1,1)
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
