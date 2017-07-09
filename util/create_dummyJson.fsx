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


let decls(keyword) = 
    parsed.GetDeclarationListInfo(Some untyped, 1, 1, "", keyword, "", (fun () -> [])) 
    |> Async.RunSynchronously

let decls_stdin(keyword) = 
    parsed.GetDeclarationListInfo(Some untyped, 2, 6, "", keyword, "", (fun () -> [])) 
    |> Async.RunSynchronously

let decls_stdout(keyword) = 
    parsed.GetDeclarationListInfo(Some untyped, 3, 7, "", keyword, "", (fun () -> [])) 
    |> Async.RunSynchronously

type JsonFormat = { word : string; info: string list list  }

let extractGroupTexts = function
    | FSharpToolTipElement.None                    -> []
    | FSharpToolTipElement.Single (a,b)            -> [a]
    | FSharpToolTipElement.SingleParameter (a,b,c) -> []
    | FSharpToolTipElement.Group xs                -> xs |> List.map fst
    | FSharpToolTipElement.CompositionError s      -> [s]

let body =
    [ []; ["System"] ; ["List"] ; ["Set"] ; ["Seq"] ; ["Array"] ; ["Map"] ; ["Option"] ; ["Observable"] ]
    |> List.map ( fun l -> 
                    let label = if List.isEmpty l then "OneWord" else List.last l
                    let body  = (decls l).Items
                                |> Array.fold ( fun state x ->
                                    let dt : JsonFormat = { word = x.Name; info = match x.DescriptionText with FSharpToolTipText xs -> List.map extractGroupTexts xs }
                                    state + " + \"\\n\" + " + "\"\"\"" +  JsonConvert.SerializeObject ( dt ) + "\"\"\"" ) ""
                                |> fun s -> s.Trim()
                    String.replicate 12 " " + "\"" + label + "\"," + body.Substring(9)
                )
    |> List.reduce ( fun a b -> a + "\n" + b )

let body_stdin =
    [["Microsoft";"FSharp";"Core";"Operators";"stdin"]]
    |> List.map ( fun l -> 
                    let label = if List.isEmpty l then "OneWord" else List.last l
                    let body  = (decls_stdin l).Items
                                |> Array.fold ( fun state x ->
                                    let dt : JsonFormat = { word = x.Name; info = match x.DescriptionText with FSharpToolTipText xs -> List.map extractGroupTexts xs }
                                    state + " + \"\\n\" + " + "\"\"\"" +  JsonConvert.SerializeObject ( dt ) + "\"\"\"" ) ""
                                |> fun s -> s.Trim()
                    String.replicate 12 " " + "\"" + label + "\"," + body.Substring(9)
                )
    |> List.reduce ( fun a b -> a + "\n" + b )

let body_stdout =
    [["Microsoft";"FSharp";"Core";"Operators";"stdout"]]
    |> List.map ( fun l -> 
                    let label = if List.isEmpty l then "OneWord" else List.last l
                    let body  = (decls_stdout l).Items
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

head + body + "\n" + body_stdout + "\n" + body_stdin + foot |> stdout.WriteLine

