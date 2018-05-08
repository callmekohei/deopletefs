// ===========================================================================
//  FILE    : deopletefs.fsx
//  AUTHOR  : callmekohei <callmekohei at gmail.com>
//  License : MIT license
// ===========================================================================

namespace deopletefs

#load @"../.paket/load/net471/main.group.fsx"
open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.SourceCodeServices
open Microsoft.FSharp.Compiler.QuickParse
open Newtonsoft.Json

open System
open System.Collections.Concurrent
open System.Diagnostics
open System.Text.RegularExpressions

#load @"./dummyJson.fsx"
open dummyJson.dummy


type PostData   = { mutable Row:int; Col:int; mutable Line:string; FilePath:string; Source:string; Init:string }
type JsonFormat = { word : string; info: string list list  }


module ServiceSettings =
    let internal getEnvInteger e dflt = match System.Environment.GetEnvironmentVariable(e) with null -> dflt | t -> try int t with _ -> dflt
    let blockingTimeout = getEnvInteger "FSharpBinding_BlockingTimeout" 500
    let maximumTimeout  = getEnvInteger "FSharpBinding_MaxTimeout" 5000


type LanguageAgent(dirtyNotify) =

    let fakeDateTimeRepresentingTimeLoaded proj = DateTime(abs (int64 (match proj with null -> 0 | _ -> proj.GetHashCode())) % 103231L)

    let checker =
        let checker = FSharpChecker.Create()
        checker.BeforeBackgroundFileCheck.Add dirtyNotify
        checker


    let parseWithTypeInfo (file, input) =
        let checkOptions, _errors   = checker.GetProjectOptionsFromScript(file, input) |> Async.RunSynchronously
        let parsingOptions, _errors = checker.GetParsingOptionsFromProjectOptions(checkOptions)
        let untypedRes              = checker.ParseFile(file, input, parsingOptions) |> Async.RunSynchronously

        match checker.CheckFileInProject(untypedRes, file, 0, input, checkOptions) |> Async.RunSynchronously with
        | FSharpCheckFileAnswer.Succeeded(res) -> untypedRes, res
        | res -> failwithf "Parsing did not finish... (%A)" res


    let getDecls (postData:PostData, partialName) = async {
            let untyped, parsed = parseWithTypeInfo (postData.FilePath, postData.Source)
            return! parsed.GetDeclarationListInfo(Some untyped, postData.Row, postData.Line, partialName, (fun () -> []))
        }


    let mbox = MailboxProcessor.Start(fun mbox ->
        async {
            while true do

                let! (postData, partialName, checkOptions, reply: AsyncReplyChannel<_> ) = mbox.Receive()

                let blockingTimeout_ms = if postData.Init = "real_init" then ServiceSettings.maximumTimeout else ServiceSettings.blockingTimeout

                let results =
                    try
                        Async.RunSynchronously (
                              getDecls( postData, partialName )
                            , timeout = blockingTimeout_ms
                        )
                        |> fun x -> Some x.Items
                    with e ->
                        None

                reply.Reply results
        }
    )

    member x.GetCheckerOptions( postData ) =
        Async.RunSynchronously (
            checker.GetProjectOptionsFromScript( postData.FilePath, postData.Source, fakeDateTimeRepresentingTimeLoaded postData.FilePath)
            , timeout = ServiceSettings.maximumTimeout
        )

    member x.GetDeclaration( postData, partialName ) =
         async {
            let opts = x.GetCheckerOptions( postData )
            return! mbox.PostAndAsyncReply(fun reply -> postData, partialName, fst(opts), reply)
        }


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
            if Array.last arr = "." then
                Array.head arr
            else
                (Array.tail arr).[0]

    let nameSpaceArrayImpl ( s:string) :string array =
        s.Split('.')
        |> Array.filter ( fun s -> s <> "" )
        |> fun arr ->
            if Array.last arr = "" then
                Array.splitAt (arr.Length - 1) arr |> fst
            else
                arr

    let nameSpaceArray (s:string) : string array =
        s
        |> fun s -> s.Split(' ')
        |> Array.filter ( fun s -> s <> "" )
        |> Array.last
        |> fun ( s:string) ->
            if s.Contains("<") then
                angleBracket s |> nameSpaceArrayImpl
            elif s.Contains(".") then
                nameSpaceArrayImpl s
            else
                [|s|]
        |> Array.map( fun s -> s.Replace("(","").Replace(")","") )

    let previousDot (s:string) =
        s.Split('.')
        |> fun arr -> Array.splitAt (Array.length arr - 1 ) arr
        |> fst
        |> Array.reduce ( fun a b -> a + "." + b )
        |> fun s -> s + "."

    // https://github.com/fsharp/FSharp.Compiler.Service/blob/master/src/fsharp/symbols/SymbolHelpers.fs#L232
    let extractGroupTexts = function
        | FSharpToolTipElement.None -> []
        | FSharpToolTipElement.CompositionError s -> [s]
        | FSharpToolTipElement.Group (xs:FSharpToolTipElementData<string> list) -> xs |> List.map( fun (x:FSharpToolTipElementData<string>) -> x.MainDescription )

    let openCount (s:string) : int =
        Regex.Matches(s,"^\s*open", RegexOptions.Multiline).Count

    /// printout message to deoplete
    let msgForDeoplete (wd:string) : string =
        JsonConvert.SerializeObject( { word = wd ; info=[[""]] } )


module  FSharpIntellisence  =
    open Util

    let jsonStrings (agent:LanguageAgent) (postData:PostData) (partialName:PartialLongName) : string =

        let x = agent.GetDeclaration( postData, partialName ) |> Async.RunSynchronously

        if Option.isNone x then
            msgForDeoplete "Parsing did not finish..."
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

        [
            "Array"
            "List"
            "Map"
            "Observable"
            "OneWordHint"
            "Option"
            "Seq"
            "Set"
            "System"
            "stderr"
            "stdin"
            "stdout"
        ]
        |> List.iter( fun label -> dic.GetOrAdd( label, dummy.Item(label)) |> ignore)


    let initSecond (agent:LanguageAgent) (dic:ConcurrentDictionary<string,string>) (postData:PostData) : Async<unit> =

        let tryUpdateDic (label:string) ( line:string) (col:int) =
            postData.Line <- line
            let partialName = QuickParse.GetPartialLongNameEx(line, col)
            dic.TryUpdate( label, jsonStrings agent postData partialName, dic.Item(label) ) |> ignore

        async{
            let tmpLine = postData.Line
            let tmpRow  = postData.Row
            postData.Row <- postData.Source.Split('\n').Length

            [
                // label          line                     col
                // ---------------------------------------------
                ( "Array"       , "Array."                , 5  )
                ( "List"        , "List."                 , 4  )
                ( "Map"         , "Map."                  , 3  )
                ( "Observable"  , "Observable."           , 10 )
                ( "OneWordHint" , ""                      , 0  )
                ( "Option"      , "Option."               , 6  )
                ( "Seq"         , "Seq."                  , 3  )
                ( "Set"         , "Set."                  , 3  )
                ( "System"      , "System."               , 6  )
                ( "stderr"      , "System.Console.Error." , 20 )
                ( "stdin"       , "System.Console.In."    , 17 )
                ( "stdout"      , "System.Console.Out."   , 18 )
            ]
            |> List.iter( fun (label,line,col) -> tryUpdateDic label line col )

            postData.Row  <- tmpRow
            postData.Line <- tmpLine
        }


    let dotHints (agent:LanguageAgent) (dic:ConcurrentDictionary<string,string>) (postData:PostData)  : string =

        let arr = nameSpaceArray postData.Line

        match Array.last arr with
        | "Array"
        | "List"
        | "Map"
        | "Observable"
        | "Option"
        | "Seq"
        | "Set"
        | "System"
        | "stderr"
        | "stdin"
        | "stdout" ->
            dic.Item( Array.last arr )
        | _ ->
            jsonStrings agent postData (QuickParse.GetPartialLongNameEx(postData.Line, postData.Col))


    let oneWordHints (dic:ConcurrentDictionary<string,string>)  (str:string) : string =

        let s =
            if Regex.Match(str,"typeof<.|.*\(.|.*\[.|.*<.|.*:.").Success then
                str.Substring( str.Length - 1 )
            else
                str.Substring(0)


        let keyword = """{"word":""" + "\"" + s.ToLower()
        dic.Item("OneWordHint")
        |> fun str -> str.Split('\n')
        |> Array.filter ( fun str -> str.ToLower().Contains( keyword ) )
        |> fun ary ->
            if Array.isEmpty ary then
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
            if Array.contains "[<" ary  && not ( Array.contains ">]" ary ) then
                attributeHints dic
            else
                oneWordHints   dic  ( Array.last ary )


    let autocomplete (s:string) (agent:LanguageAgent) ( dic :ConcurrentDictionary<string,string> )  : string =

        let postData = JsonConvert.DeserializeObject<PostData>(s)

        let tryUpdateDic (label:string) ( line:string) (col:int) =
            postData.Line <- line
            let partialName = QuickParse.GetPartialLongNameEx(line, col)
            dic.TryUpdate( label, jsonStrings agent postData partialName, dic.Item(label) ) |> ignore

        // update dictionary when open keyword appears.
        let updateOpenKeyword () =
            if ( openCount( postData.Line ) < 1 ) && ( int(dic.Item("openCount")) <> openCount( postData.Source ) ) then
                Debug.WriteLine("changed!")

                let tmpLine = postData.Line
                let tmpRow  = postData.Row
                postData.Row <- postData.Source.Split('\n').Length

                dic.TryUpdate( "openCount", string( openCount( postData.Source )), dic.Item("openCount") ) |> ignore

                [ ("OneWordHint","",0);("System","System.",6);("Observable","Observable.",10)]
                |> List.iter( fun (label,line,col) -> tryUpdateDic label line col )

                postData.Row  <- tmpRow
                postData.Line <- tmpLine


        let main () =

            // For Windows
            // System.Threading.Thread.Sleep(25)

            updateOpenKeyword ()

            let s = postData.Line.Replace("("," ").Split(' ')
                    |> Array.filter ( fun s -> s <> "" )
                    |> Array.last

            /// . .. ... .... List.. List... List....
            if  s.StartsWith(".") || (Regex.Match(s,"^.*\.\.+?$")).Success then
                msgForDeoplete ""
            else
                if s.Contains(".") then
                    if  s.EndsWith(".") then
                        dotHints agent dic postData
                    else
                        postData.Line <- previousDot( s )
                        dotHints agent dic postData
                else
                    oneWordOrAttributeHints dic postData


        if postData.Init = "dummy_init" then
            Debug.WriteLine("dummy_init initialize")
            initfirst agent dic postData
            Debug.WriteLine("finish dummy_init initialize")
            msgForDeoplete "finish dummy initialize!"
        elif postData.Init = "real_init" then
            Debug.WriteLine("real_init initialize")
            initSecond agent dic postData |> Async.Start
            Debug.WriteLine("finish real_init initialize")
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
