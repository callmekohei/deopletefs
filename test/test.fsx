#r "../bin_deopletefs/deopletefs.exe"
open deopletefs.Util
open deopletefs.InteractiveConsole
open deopletefs.FSharpIntellisence

#r "../packages/Newtonsoft.Json/lib/net45/Newtonsoft.Json.dll"
open Newtonsoft.Json

#r "../packages/Persimmon/lib/net45/Persimmon.dll"
#r "../packages/Persimmon.Runner/lib/net40/Persimmon.Runner.dll"
#r "../packages/Persimmon.Script/lib/net45/Persimmon.Script.dll"
open System.Reflection
open Persimmon
open UseTestNameByReflection


let ``test of angleBracket`` = test {
    // do! assertEquals "abc" ( angleBracket "abc" )
    // do! assertEquals "abc" ( angleBracket "typeof<" )
    do! assertEquals "System"      ( angleBracket "typeof<System" )
    do! assertEquals "System."     ( angleBracket "typeof<System." )
    do! assertEquals "System.Math" ( angleBracket "typeof<System.Math" )
    do! assertEquals "System.Math" ( angleBracket "typeof<System.Math>" )
    do! assertEquals "typeof"      ( angleBracket "typeof<System.Math>." )
}

let ``test of nameSpaceArrayImpl`` = test {
    do! assertEquals [|"System"|]        ( nameSpaceArrayImpl "System" )
    do! assertEquals [|"System"|]        ( nameSpaceArrayImpl "System." )
    do! assertEquals [|"System";"Text"|] ( nameSpaceArrayImpl "System.Text" )
    do! assertEquals [|"System";"Text"|] ( nameSpaceArrayImpl "System.Text." )
}

let ``test of nameSpaceArray`` = test {
    do! assertEquals [|"System"|]        ( nameSpaceArrayImpl "System" )
    do! assertEquals [|"System"|]        ( nameSpaceArrayImpl "System." )
    do! assertEquals [|"System";"Text"|] ( nameSpaceArrayImpl "System.Text" )
    do! assertEquals [|"System";"Text"|] ( nameSpaceArrayImpl "System.Text." )

    do! assertEquals "System.Math" ( angleBracket "typeof<System.Math" )
    do! assertEquals "System.Math" ( angleBracket "typeof<System.Math>" )
    do! assertEquals "typeof"      ( angleBracket "typeof<System.Math>." )
}

let ``test of previousDot`` = test {
    // do! assertEquals "abc" ( previousDot "System" ) // error
    do! assertEquals "System."                               ( previousDot "System.Text")
    do! assertEquals "System.Text."                          ( previousDot "System.Text.RegularExpressions")
    do! assertEquals "System.Text.RegularExpressions."       ( previousDot "System.Text.RegularExpressions.Regex")
    do! assertEquals "System.Text.RegularExpressions.Regex." ( previousDot "System.Text.RegularExpressions.Regex.Split")
}

let ``test of openCount`` = test {
    do! assertEquals 0 (openCount "")
    do! assertEquals 1 (openCount "open")
    do! assertEquals 2 (openCount "open \n open")
    do! assertEquals 3 (openCount "open \n open \n open")
}

let ``test of msgForDeoplete`` = test {
    do! assertEquals """{"word":"abc","info":[[""]]}""" ( msgForDeoplete "abc" )
}

let ``test of autocomplete`` = test {

    let json = """{ "Row" : -9999 ,"Col": -9999, "Line": "", "FilePath" : "./dummy.fsx", "Source" : "", "Init":"dummy_init"}"""
    do! assertEquals """{"word":"finish dummy initialize!","info":[[""]]}""" ( autocomplete json agent dic )

    let json2 = """{ "Row" : 1 ,"Col": 1, "Line": "", "FilePath" : "./dummy.fsx", "Source" : "", "Init":"real_init"}"""
    do! assertEquals """{"word":"finish real initialize!","info":[[""]]}""" ( autocomplete json2 agent dic )

    let json3 = """{ "Row" : 1 ,"Col": 5, "Line": "List.", "FilePath" : "./dummy.fsx", "Source" : "", "Init":"false"}"""
    let ListFuncs = ( autocomplete json3 agent dic ).Split('\n') |> fun ary -> ary.[0]
    do! assertEquals "allPairs" ( JsonConvert.DeserializeObject<deopletefs.JsonFormat>(ListFuncs).word )

    let json4 = """{ "Row" : 1 ,"Col": 6, "Line": "stdin.", "FilePath" : "./dummy.fsx", "Source" : "", "Init":"false"}"""
    let stdinMethods = ( autocomplete json4 agent dic ).Split('\n') |> fun ary -> ary.[0]
    do! assertEquals "Close" ( JsonConvert.DeserializeObject<deopletefs.JsonFormat>(stdinMethods).word )

    let json5 = """{ "Row" : 1 ,"Col": 2, "Line": "[<", "FilePath" : "./dummy.fsx", "Source" : "", "Init":"false"}"""
    let attributeA = ( autocomplete json5 agent dic ).Split('\n') |> fun ary -> ary.[0]
    do! assertEquals "AbstractClassAttribute" ( JsonConvert.DeserializeObject<deopletefs.JsonFormat>(attributeA).word )

    let json6 = """{ "Row" : 1 ,"Col": 1, "Line": "a", "FilePath" : "./dummy.fsx", "Source" : "", "Init":"false"}"""
    let filteredByA = ( autocomplete json6 agent dic ).Split('\n') |> fun ary -> ary.[0]
    do! assertEquals "abs" ( JsonConvert.DeserializeObject<deopletefs.JsonFormat>(filteredByA).word )

}

/// print out test report.
new Persimmon.ScriptContext()
|> FSI.collectAndRun( fun _ -> Assembly.GetExecutingAssembly() )
