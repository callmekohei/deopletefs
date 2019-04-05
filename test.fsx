#load @"./.paket/load/net472/main.group.fsx"
#r "./bin/deopletefs.exe"

open System.Diagnostics
open System.Collections.Concurrent

open Newtonsoft.Json
open deopletefs.Util
open deopletefs.InteractiveConsole
open deopletefs.FSharpIntellisence

let check  x y  =
  if x = y then
    "OK!: "|> stdout.WriteLine
  else
    "NG!: "|> stdout.WriteLine

let encode64 (s:string) =
    System.Convert.ToBase64String( System.Text.Encoding.UTF8.GetBytes( s ) )

let decode64 (base64String:string) =
    System.Text.Encoding.UTF8.GetString( System.Convert.FromBase64String( base64String ) )

type Interaction (fp:string, args:string, wd:string) =

  let cq = new ConcurrentQueue<DataReceivedEventArgs>()

  // 子プロセスの起動パラメータを作成する
  let procStartInfo =
    ProcessStartInfo(
      FileName         = fp   ,
      Arguments        = args ,
      WorkingDirectory     = wd   ,
      RedirectStandardInput  = true , // 子プロセスの標準入力をリダイレクトする
      RedirectStandardOutput = true , // 子プロセスの標準出力をリダイレクトする
      RedirectStandardError  = true , // 子プロセスの標準エラーをリダイレクトする
      UseShellExecute      = false  // シェルを使用せず子プロセスを起動する(リダイレクトするために必要)
  )

  // 子プロセスをインスタンス化
  let ps = new Process(StartInfo = procStartInfo)

  do ps.OutputDataReceived
    |> Event.add ( fun (args :DataReceivedEventArgs) ->
      cq.Enqueue(args) |> ignore
      // check on travis with my eyes :-)
      // stdout.WriteLine(args.Data )|> ignore
    )

  // 子プロセスを起動する
  do ps.Start() |> ignore
  // 子プロセスがアイドル状態になるまで無期限に待機
  do ps.WaitForInputIdle() |>ignore
  // Start the asynchronous read of the ps output stream.
  // 標準出力の非同期読み込みを開始する
  do ps.BeginOutputReadLine()

  // リダイレクトした子プロセスの標準入力にテキストを書き込む
  member this.Send(message:string) =
    ps.StandardInput.WriteLine(message)

  // 子プロセスを終了する
  member this.Quit() =
    ps.Kill()

  member this.recieveData() :DataReceivedEventArgs =
    snd ( cq.TryDequeue() )


let mono  = "/usr/local/Cellar/mono/5.18.0.268/bin/mono"
let args  = "./bin/deopletefs.exe"
let workdir = "./"
let replyer = Interaction(mono,args,workdir)

let test_of_angleBracket () =
    // do! assertEquals "abc" ( angleBracket "abc" )
    // do! assertEquals "abc" ( angleBracket "typeof<" )
    check "System"      ( angleBracket "typeof<System" )
    check "System."     ( angleBracket "typeof<System." )
    check "System.Math" ( angleBracket "typeof<System.Math" )
    check "System.Math" ( angleBracket "typeof<System.Math>" )
    check "typeof"      ( angleBracket "typeof<System.Math>." )
test_of_angleBracket ()

let test_of_nameSpaceArrayImpl () =
    check [|"System"|]        ( nameSpaceArrayImpl "System" )
    check [|"System"|]        ( nameSpaceArrayImpl "System." )
    check [|"System";"Text"|] ( nameSpaceArrayImpl "System.Text" )
    check [|"System";"Text"|] ( nameSpaceArrayImpl "System.Text." )
test_of_nameSpaceArrayImpl

let test_of_nameSpaceArray =
    check [|"System"|]        ( nameSpaceArrayImpl "System" )
    check [|"System"|]        ( nameSpaceArrayImpl "System." )
    check [|"System";"Text"|] ( nameSpaceArrayImpl "System.Text" )
    check [|"System";"Text"|] ( nameSpaceArrayImpl "System.Text." )
    check "System.Math" ( angleBracket "typeof<System.Math" )
    check "System.Math" ( angleBracket "typeof<System.Math>" )
    check "typeof"      ( angleBracket "typeof<System.Math>." )

let test_of_previousDot =
    // do! assertEquals "abc" ( previousDot "System" ) // error
    check "System."                               ( previousDot "System.Text")
    check "System.Text."                          ( previousDot "System.Text.RegularExpressions")
    check "System.Text.RegularExpressions."       ( previousDot "System.Text.RegularExpressions.Regex")
    check "System.Text.RegularExpressions.Regex." ( previousDot "System.Text.RegularExpressions.Regex.Split")

let test_of_openCount =
    check 0 (openCount "")
    check 1 (openCount "open")
    check 2 (openCount "open \n open")
    check 3 (openCount "open \n open \n open")

let test_of_msgForDeoplete =
    check """{"word":"abc","info":[[""]]}""" ( decode64( msgForDeoplete "abc" ) )


let initialize_deopleteExe () =
  let json = """{ "Row" : -9999 ,"Col": 1, "Line": "", "FilePath" : "./dummy.fsx", "Source" : "", "Init":"true"}"""
  let resultString = """{"word":"finish initialize!","info":[[""]]}"""
  replyer.Send(json)
  // wait for lunch time :-)  It's imortant!
  System.Threading.Thread.Sleep 10000
  check (decode64( replyer.recieveData().Data )) resultString

initialize_deopleteExe ()

stdout.WriteLine " ------------------------------------ "

let List_Cons () =
    let json           = """{ "Row" : 1 ,"Col": 5, "Line": "List.", "FilePath" : "./dummy.fsx", "Source" : "", "Init":"false"}"""
    replyer.Send(json)
    System.Threading.Thread.Sleep 500
    let ListFuncs      = ( decode64( replyer.recieveData().Data ) ).Split('\n') |> fun ary -> ary.[0]
    let resultString   = JsonConvert.DeserializeObject<deopletefs.JsonFormat>(ListFuncs).word
    let expectedString = "Cons"
    check expectedString resultString
List_Cons ()

let stdin_Close =
    let json           = """{ "Row" : 1 ,"Col": 6, "Line": "stdin.", "FilePath" : "./dummy.fsx", "Source" : "", "Init":"false"}"""
    replyer.Send(json)
    System.Threading.Thread.Sleep 500
    let stdinMethods   = ( decode64( replyer.recieveData().Data ) ).Split('\n') |> fun ary -> ary.[0]
    let resultString   = JsonConvert.DeserializeObject<deopletefs.JsonFormat>(stdinMethods).word
    let expectedString = "Close"
    check expectedString resultString

let oneWordHints_AbstractClassAttribute =
    let json           = """{ "Row" : 1 ,"Col": 1, "Line": "a", "FilePath" : "./dummy.fsx", "Source" : "", "Init":"false"}"""
    replyer.Send(json)
    System.Threading.Thread.Sleep 500
    let filteredByA    = ( decode64( replyer.recieveData().Data ) ).Split('\n') |> fun ary -> ary.[0]
    let resultString   = JsonConvert.DeserializeObject<deopletefs.JsonFormat>(filteredByA).word
    let expectedString = "AbstractClassAttribute"
    check expectedString resultString


let attributeHints_AbstractClassAttribute =
    let json           = """{ "Row" : 1 ,"Col": 2, "Line": "[<", "FilePath" : "./dummy.fsx", "Source" : "", "Init":"false"}"""
    replyer.Send(json)
    System.Threading.Thread.Sleep 500
    let attributeA     = ( decode64( replyer.recieveData().Data ) ).Split('\n') |> fun ary -> ary.[0]
    let resultString   = JsonConvert.DeserializeObject<deopletefs.JsonFormat>(attributeA).word
    let expectedString = "AbstractClassAttribute"
    check expectedString resultString

