// ===========================================================================
//  FILE    : test.fsx
//  AUTHOR  : callmekohei <callmekohei at gmail.com>
//  License : MIT license
// ===========================================================================

open System
open System.IO

#load @"../.paket/load/net471/main.group.fsx"
open Newtonsoft.Json
open System.Reflection
open Persimmon
open UseTestNameByReflection

#r "../bin/deopletefs.exe"
open deopletefs.Util
open deopletefs.InteractiveConsole
open deopletefs.FSharpIntellisence


let encode64 (s:string) =
    System.Convert.ToBase64String( System.Text.Encoding.UTF8.GetBytes( s ) )

let decode64 (base64String:string) =
    System.Text.Encoding.UTF8.GetString( System.Convert.FromBase64String( base64String ) )


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
    do! assertEquals """{"word":"abc","info":[[""]]}""" ( decode64( msgForDeoplete "abc" ) )
}

let ``initialize`` = test {
    let json           = """{ "Row" : -9999 ,"Col": 1, "Line": "", "FilePath" : "./dummy.fsx", "Source" : "", "Init":"true"}"""
    let resultString   = decode64 ( autocomplete json agent dic )
    let expectedString = """{"word":"finish initialize!","info":[[""]]}"""
    do! assertEquals expectedString resultString
}

let ``List - Cons`` = test {
    let json           = """{ "Row" : 1 ,"Col": 5, "Line": "List.", "FilePath" : "./dummy.fsx", "Source" : "", "Init":"false"}"""
    let ListFuncs      = ( decode64( autocomplete json agent dic ) ).Split('\n') |> fun ary -> ary.[0]
    let resultString   = JsonConvert.DeserializeObject<deopletefs.JsonFormat>(ListFuncs).word
    let expectedString = "Cons"
    do! assertEquals expectedString resultString
}

let ``stdin - Close`` = test {
    let json           = """{ "Row" : 1 ,"Col": 6, "Line": "stdin.", "FilePath" : "./dummy.fsx", "Source" : "", "Init":"false"}"""
    let stdinMethods   = ( decode64( autocomplete json agent dic ) ).Split('\n') |> fun ary -> ary.[0]
    let resultString   = JsonConvert.DeserializeObject<deopletefs.JsonFormat>(stdinMethods).word
    let expectedString = "Close"
    do! assertEquals expectedString resultString
}

let ``oneWordHints - AbstractClassAttribute`` = test {
    let json           = """{ "Row" : 1 ,"Col": 1, "Line": "a", "FilePath" : "./dummy.fsx", "Source" : "", "Init":"false"}"""
    let filteredByA    = ( decode64( autocomplete json agent dic ) ).Split('\n') |> fun ary -> ary.[0]
    let resultString   = JsonConvert.DeserializeObject<deopletefs.JsonFormat>(filteredByA).word
    let expectedString = "AbstractClassAttribute"
    do! assertEquals expectedString resultString
}

let ``attributeHints - AbstractClassAttribute`` = test {
    let json           = """{ "Row" : 1 ,"Col": 2, "Line": "[<", "FilePath" : "./dummy.fsx", "Source" : "", "Init":"false"}"""
    let attributeA     = ( decode64( autocomplete json agent dic ) ).Split('\n') |> fun ary -> ary.[0]
    let resultString   = JsonConvert.DeserializeObject<deopletefs.JsonFormat>(attributeA).word
    let expectedString = "AbstractClassAttribute"
    do! assertEquals expectedString resultString
}

/// print out test report.
new Persimmon.ScriptContext()
|> FSI.collectAndRun( fun _ -> Assembly.GetExecutingAssembly() )
