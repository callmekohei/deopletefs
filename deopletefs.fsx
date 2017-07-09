// ===========================================================================
//  FILE    : deopletefs.fsx
//  AUTHOR  : callmekohei <callmekohei at gmail.com>
//  License : MIT license
// ===========================================================================

namespace deopletefs
#load @"./util/dummyJson.fsx"

open System
open System.Collections.Concurrent
open System.Diagnostics
open System.Text.RegularExpressions

#r @"./packages/FSharp.Compiler.Service/lib/net45/FSharp.Compiler.Service.dll"
open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.SourceCodeServices

#r @"./packages/Newtonsoft.Json/lib/net45/Newtonsoft.Json.dll"
open Newtonsoft.Json


type PostData   = { mutable Row:int; mutable Col:int; mutable Line:string; FilePath:string; mutable Source:string; Init:string }
type JsonFormat = { word : string; info: string list list  }


module ServiceSettings =
    let internal getEnvInteger e dflt = match System.Environment.GetEnvironmentVariable(e) with null -> dflt | t -> try int t with _ -> dflt
    let blockingTimeout = getEnvInteger "FSharpBinding_BlockingTimeout" 250
    let maximumTimeout  = getEnvInteger "FSharpBinding_MaxTimeout" 5000


type LanguageAgent(dirtyNotify) =
    
    let fakeDateTimeRepresentingTimeLoaded proj = DateTime(abs (int64 (match proj with null -> 0 | _ -> proj.GetHashCode())) % 103231L)

    let checker = 
        let checker = FSharpChecker.Create()
        checker.BeforeBackgroundFileCheck.Add dirtyNotify
        checker

    let mbox = MailboxProcessor.Start(fun mbox ->
        async { 
             while true do
                
                let! (postData, arr, checkOptions, reply: AsyncReplyChannel<_> ) = mbox.Receive()

                let blockingTimeout_ms = if postData.Init = "real_init" then ServiceSettings.maximumTimeout else ServiceSettings.blockingTimeout

                let! untypedRes, typedRes = checker.GetBackgroundCheckResultsForFileInProject( postData.FilePath, checkOptions )

                let ary =
                    try
                        Async.RunSynchronously (
                            typedRes.GetDeclarationListInfo ( Some untypedRes, postData.Row , postData.Col , postData.Line, (fst arr) |> Array.toList, (snd arr), fun () -> [] )
                            , timeout = blockingTimeout_ms
                        )
                        |> fun x -> Some x
                    with e -> None

                let results =
                    if      Option.isSome ary && not (Array.isEmpty (Option.get ary |> fun x -> x.Items))
                    then    Some (Option.get ary |> fun x -> x.Items)
                    else
                            let untyped     = checker.ParseFileInProject(postData.FilePath, postData.Source, checkOptions)              |> Async.RunSynchronously
                            let checkAnswer = checker.CheckFileInProject(untyped, postData.FilePath, 0, postData.Source, checkOptions ) |> Async.RunSynchronously
                            
                            match checkAnswer with
                            | FSharpCheckFileAnswer.Succeeded(typed) ->
                                let ary =
                                    try 
                                        Async.RunSynchronously (
                                            typed.GetDeclarationListInfo ( Some untyped, postData.Row , postData.Col , postData.Line, (fst arr) |> Array.toList, (snd arr), fun () -> [] )
                                            , timeout = blockingTimeout_ms
                                        )
                                        |> fun x -> Some x
                                    with e -> None

                                if      Option.isSome ary && not (Array.isEmpty (Option.get ary |> fun x -> x.Items))
                                then    Some (Option.get ary |> fun x -> x.Items)
                                else    None 

                            | _ -> None
                            
                reply.Reply results
        } 
    )

    member x.GetCheckerOptions( postData ) =
        Async.RunSynchronously (
            checker.GetProjectOptionsFromScript( postData.FilePath, postData.Source, fakeDateTimeRepresentingTimeLoaded postData.FilePath)
            , timeout = ServiceSettings.maximumTimeout
        )
     
    member x.GetDeclaration( postData, arr ) =
         async {
            let opts = x.GetCheckerOptions( postData )
            return! mbox.PostAndAsyncReply(fun reply -> postData, arr, fst(opts), reply)
         }

    member x.ClearLanguageServiceRootCachesAndCollectAndFinalizeAllTransients() =
        checker.ClearLanguageServiceRootCachesAndCollectAndFinalizeAllTransients


type PrinterAgent() =
    let inbox = MailboxProcessor.Start( fun inbox ->
        let rec loop () = async {
            let! ( msg : Choice< string , AsyncReplyChannel<unit> > ) = inbox.Receive()
            match msg with
            | Choice1Of2 (s:string) ->
                stdout.WriteLine(s)
                return! loop ()
            | Choice2Of2 (ch : AsyncReplyChannel<unit>) -> ch.Reply ()
        }
        loop ()
    )

    member x.WriteLine(s) = inbox.Post (Choice1Of2 s )
    member x.Quit()       = inbox.PostAndReply(fun ch -> Choice2Of2 ch)


module Util =


    let angleBracket ( s:string ) :string =
        s.Split([|'<';'>'|])
        |> Array.filter ( fun s -> s <> "" )
        |> fun arr ->
           if     Array.last arr = "."
           then   Array.head arr
           else   (Array.tail arr).[0]

    let nameSpaceArrayImpl ( s:string) :string array =
        s.Split('.')
        |> Array.filter ( fun s -> s <> "" )
        |> fun arr ->
            if    Array.last arr = ""
            then  Array.splitAt (arr.Length - 1) arr |> fst
            else  arr

    let nameSpaceArray (s:string) : string array =
        s
        |> fun s -> s.Split(' ')
        |> Array.filter ( fun s -> s <> "" )
        |> Array.last
        |> fun ( s:string) ->
            if    s.Contains("<")
            then  angleBracket s |> nameSpaceArrayImpl
            elif  s.Contains(".")
            then  nameSpaceArrayImpl s
            else  [|s|]
        |> Array.map( fun s -> s.Replace("(","").Replace(")","") )

    let extractGroupTexts = function
        | FSharpToolTipElement.None                    -> []
        | FSharpToolTipElement.Single (a,b)            -> [a]
        | FSharpToolTipElement.SingleParameter (a,b,c) -> []
        | FSharpToolTipElement.Group xs                -> xs |> List.map fst
        | FSharpToolTipElement.CompositionError s      -> [s]

    let openCount (s:string) : int =
        Regex.Matches(s,"^\s*open", RegexOptions.Multiline).Count

    /// printout message to deoplete
    let msgForDeoplete (wd:string) : string =
        JsonConvert.SerializeObject( { word = wd ; info=[[""]] } )


module  FSharpIntellisence  =
    open Util

    let jsonStrings (agent:LanguageAgent) (postData:PostData) (nameSpace: string [] )  (word:string) : string =
        
        let x = agent.GetDeclaration( postData, ( nameSpace, word ) ) |> Async.RunSynchronously

        if      Option.isNone x
        then    msgForDeoplete "Parsing did not finish..."
        else
                x.Value
                |> Array.fold ( fun state x ->
                    let dt : JsonFormat = { word = x.Name; info = match x.DescriptionText with FSharpToolTipText xs -> List.map extractGroupTexts xs }
                    state + "\n" + JsonConvert.SerializeObject ( dt )
                    ) ""
                |> fun s -> s.Trim()


    let initfirst (agent:LanguageAgent) (dic:ConcurrentDictionary<string,string>) (postData:PostData) : unit =

        dic.GetOrAdd( "filePath"    , postData.FilePath )                       |> ignore
        dic.GetOrAdd( "openCount"   , string(openCount(postData.Source)) )      |> ignore
        dic.GetOrAdd( "DotHints"    , msgForDeoplete "" )                       |> ignore

        dic.GetOrAdd( "System"      , dummyJson.dummy.dummy.Item("System"))     |> ignore
        dic.GetOrAdd( "OneWordHint" , dummyJson.dummy.dummy.Item("OneWord"))    |> ignore
        dic.GetOrAdd( "List"        , dummyJson.dummy.dummy.Item("List"))       |> ignore
        dic.GetOrAdd( "Set"         , dummyJson.dummy.dummy.Item("Set"))        |> ignore
        dic.GetOrAdd( "Seq"         , dummyJson.dummy.dummy.Item("Seq"))        |> ignore
        dic.GetOrAdd( "Array"       , dummyJson.dummy.dummy.Item("Array"))      |> ignore
        dic.GetOrAdd( "Map"         , dummyJson.dummy.dummy.Item("Map"))        |> ignore
        dic.GetOrAdd( "Option"      , dummyJson.dummy.dummy.Item("Option"))     |> ignore
        dic.GetOrAdd( "Observable"  , dummyJson.dummy.dummy.Item("Observable")) |> ignore
        dic.GetOrAdd( "stdout"      , dummyJson.dummy.dummy.Item("stdout"))     |> ignore
        dic.GetOrAdd( "stdin"       , dummyJson.dummy.dummy.Item("stdin"))      |> ignore


    let initSecond (agent:LanguageAgent) (dic:ConcurrentDictionary<string,string>) (postData:PostData) : Async<unit> =
        async{
            dic.TryUpdate( "System"      , jsonStrings agent postData [|"System"|] "", dic.Item("System")      ) |> ignore
            dic.TryUpdate( "OneWordHint" , jsonStrings agent postData [||] ""        , dic.Item("OneWordHint") ) |> ignore

            [| "List" ; "Set" ; "Seq" ; "Array" ; "Map" ; "Option" ; "Observable" |]
            |> Array.iter ( fun (s:string) ->
                dic.TryUpdate( s, jsonStrings agent postData [|s|] "", dic.Item(s) ) |> ignore )

            let tmpRow    = postData.Row
            let tmpCol    = postData.Col
            let tmpLine   = postData.Line
            let tmpSource = postData.Source

            postData.Row    <- 1 
            postData.Col    <- 7
            postData.Line   <- "stdout."
            postData.Source <- "stdout." 
            dic.TryUpdate( "stdout" , jsonStrings agent postData [|"Microsoft";"FSharp";"Core";"Operators";"stdout"|] "" , dic.Item("stdout") ) |> ignore

            postData.Row    <- 1 
            postData.Col    <- 6
            postData.Line   <- "stdin."
            postData.Source <- "stdin."
            dic.TryUpdate( "stdin"  , jsonStrings agent postData [|"Microsoft";"FSharp";"Core";"Operators";"stdin"|]  "" , dic.Item("stdin")  ) |> ignore

            postData.Row    <- tmpRow
            postData.Col    <- tmpCol
            postData.Line   <- tmpLine
            postData.Source <- tmpSource
        }


    let dotHints (agent:LanguageAgent) (dic:ConcurrentDictionary<string,string>) (postData:PostData)  : string =

        let arr = nameSpaceArray postData.Line

        match Array.last arr with
        | "System" | "List" | "Set" | "Seq" | "Array" | "Map" | "Option" |"Observable" | "stdout" | "stdin" ->
            dic.Item( Array.last arr )
        | _ ->
            jsonStrings agent postData arr ""

    let oneWordHints (dic:ConcurrentDictionary<string,string>)  (str:string) : string =
        
        let s =
            if      Regex.Match(str,"typeof<.").Success
            then    str.Substring( str.Length - 1 )
            else    str.Substring(0)


        let keyword = """{"word":""" + "\"" + s.ToLower()
        dic.Item("OneWordHint")
        |> fun str -> str.Split('\n')
        |> Array.filter ( fun str -> str.ToLower().Contains( keyword ) )
        |> fun ary -> 
            if      Array.isEmpty ary
            then   
                    ""
            else
                    ary |> Array.reduce ( fun a b -> a + "\n" + b )
                        |> fun s -> s.TrimEnd()


    let attributeHints (dic:ConcurrentDictionary<string,string>) : string =
        dic.Item( "OneWordHint" )
        |> fun str -> str.Split('\n')
        |> Array.filter ( fun str -> str.Contains("Attribute") )
        |> Array.reduce ( fun a b -> a + "\n" + b )
        |> fun s -> s.TrimEnd()


    let oneWordOrAttributeHints (dic:ConcurrentDictionary<string,string>) (postData:PostData) : string =
        postData.Line.Split(' ')
        |> Array.filter ( fun s -> s <> "" )
        |> fun ary ->
               if       Array.contains "[<" ary  && not ( Array.contains ">]" ary )
               then     attributeHints dic
               else
                        oneWordHints   dic  ( Array.last ary )


    let autocomplete (s:string) (agent:LanguageAgent) ( dic :ConcurrentDictionary<string,string> )  : string =
        
        /// postData.Col is cursor position. not . position! ( eg. "abc". ---> postData.Col:7 . position: 6 )
        let postData = JsonConvert.DeserializeObject<PostData>(s)

        let main () =

            /// update condition of open keyword
            if      ( openCount( postData.Line ) < 1 ) && ( int(dic.Item("openCount")) <> openCount( postData.Source ) )
            then 

                    dic.TryUpdate( "openCount"  , string( openCount( postData.Source ))          , dic.Item("openCount")  )  |> ignore 
                    dic.TryUpdate( "Observable" , jsonStrings agent postData [|"Observable"|] "" , dic.Item("Observable")  ) |> ignore 
                    dic.TryUpdate( "OneWordHint", jsonStrings agent postData [||] ""             , dic.Item("OneWordHint") ) |> ignore
            
            let s = postData.Line.Replace("("," ").Split(' ')
                    |> Array.filter ( fun s -> s <> "" )
                    |> Array.last

            if         s.StartsWith(".")                       /// . .. ... ....
                    || (Regex.Match(s,"^.*\.\.+?$")).Success   /// List.. List... List....
            then
                    msgForDeoplete "" 
            else
                    if      s.Contains(".")
                    then
                            if      s.EndsWith(".")
                            then   
                                    postData.Col <- postData.Col - 1 // to . position from cursor position.
                                    dotHints agent dic postData
                            else    s.Split('.')
                                    |> fun arr -> Array.splitAt (Array.length arr - 1 ) arr
                                    |> fst
                                    |> Array.reduce ( fun a b -> a + "." + b )
                                    |> fun s -> s + "."
                                    |> fun str -> postData.Line <- str
                                    dotHints agent dic postData
                    else    
                            oneWordOrAttributeHints dic postData

        if      postData.Init = "dummy_init"
        then    initfirst agent dic postData
                msgForDeoplete "finish dummy initialize!"
        elif    postData.Init = "real_init"
        then    initSecond agent dic postData |> Async.Start
                msgForDeoplete "finish real initialize!"
        else   
                main () 


module InteractiveConsole =
    open Util
    open FSharpIntellisence

    #if DEBUG
    let listener  = new DefaultTraceListener()
    let parentDic = System.Reflection.Assembly.GetEntryAssembly().Location |> fun x -> string ( System.IO.Directory.GetParent(x) )
    listener.LogFileName <- System.IO.Path.Combine( parentDic, "log.txt" )
    Debug.Listeners.Add(listener)
    #endif


    let printer = new PrinterAgent()
    let dic : ConcurrentDictionary<string,string> = new ConcurrentDictionary< string, string >()
    let agent = new LanguageAgent(fun _ -> () )


    let rec main s =
        match s with
        | "quit" -> printer.Quit()
        | "init" -> main ( stdin.ReadLine() )
        | _      -> printer.WriteLine(autocomplete s agent dic)
                    main ( stdin.ReadLine() )

    [<EntryPointAttribute>]
    let entry arg =
        main "init"
        0
