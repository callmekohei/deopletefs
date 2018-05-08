#r "../bin/deopletefs.exe"
open deopletefs.Util
open deopletefs.InteractiveConsole
open deopletefs.FSharpIntellisence

// #load @"../.paket/load/net471/main.group.fsx"
#r "../packages/Newtonsoft.Json/lib/net45/Newtonsoft.Json.dll"
#r "../packages/Microsoft.DiaSymReader/lib/net20/Microsoft.DiaSymReader.dll"
#r "../packages/System.Collections.Immutable/lib/netstandard2.0/System.Collections.Immutable.dll"
#r "../packages/System.ValueTuple/lib/net47/System.ValueTuple.dll"
#r "../packages/System.Net.Http/lib/net46/System.Net.Http.dll"
#r "../packages/System.Security.Cryptography.X509Certificates/lib/net461/System.Security.Cryptography.X509Certificates.dll"
#r "../packages/System.Security.Cryptography.Cng/lib/net47/System.Security.Cryptography.Cng.dll"
#r "../packages/System.Security.Cryptography.Csp/lib/net46/System.Security.Cryptography.Csp.dll"
#r "../packages/System.Security.Cryptography.OpenSsl/lib/netstandard2.0/System.Security.Cryptography.OpenSsl.dll"
#r "../packages/System.ComponentModel.TypeConverter/lib/net462/System.ComponentModel.TypeConverter.dll"
#r "../packages/System.IO.Compression.ZipFile/lib/net46/System.IO.Compression.ZipFile.dll"
#r "../packages/System.Security.Cryptography.Algorithms/lib/net463/System.Security.Cryptography.Algorithms.dll"
#r "../packages/System.Xml.XmlDocument/lib/net46/System.Xml.XmlDocument.dll"
#r "../packages/System.Collections.Specialized/lib/net46/System.Collections.Specialized.dll"
#r "../packages/System.IO.Compression/lib/net46/System.IO.Compression.dll"
#r "../packages/System.Linq.Expressions/lib/net463/System.Linq.Expressions.dll"
#r "../packages/System.Runtime.Serialization.Formatters/lib/net46/System.Runtime.Serialization.Formatters.dll"
#r "../packages/System.Security.Cryptography.Encoding/lib/net46/System.Security.Cryptography.Encoding.dll"
#r "../packages/System.Xml.ReaderWriter/lib/net46/System.Xml.ReaderWriter.dll"
#r "../packages/System.Buffers/lib/netstandard2.0/System.Buffers.dll"
#r "../packages/System.Collections.NonGeneric/lib/net46/System.Collections.NonGeneric.dll"
#r "../packages/System.ComponentModel.Primitives/lib/net45/System.ComponentModel.Primitives.dll"
#r "../packages/System.Diagnostics.Process/lib/net461/System.Diagnostics.Process.dll"
#r "../packages/System.Diagnostics.TraceSource/lib/net46/System.Diagnostics.TraceSource.dll"
#r "../packages/System.Globalization.Extensions/lib/net46/System.Globalization.Extensions.dll"
#r "../packages/System.Linq/lib/net463/System.Linq.dll"
#r "../packages/System.Runtime.InteropServices.RuntimeInformation/lib/net45/System.Runtime.InteropServices.RuntimeInformation.dll"
#r "../packages/System.Runtime.Serialization.Primitives/lib/net46/System.Runtime.Serialization.Primitives.dll"
#r "../packages/System.Security.Cryptography.Primitives/lib/net46/System.Security.Cryptography.Primitives.dll"
#r "../packages/System.Text.RegularExpressions/lib/net463/System.Text.RegularExpressions.dll"
#r "../packages/System.Diagnostics.DiagnosticSource/lib/net46/System.Diagnostics.DiagnosticSource.dll"
#r "../packages/System.Runtime.InteropServices/lib/net463/System.Runtime.InteropServices.dll"
#r "../packages/System.Console/lib/net46/System.Console.dll"
#r "../packages/System.IO.FileSystem/lib/net46/System.IO.FileSystem.dll"
#r "../packages/System.Net.Sockets/lib/net46/System.Net.Sockets.dll"
#r "../packages/System.Reflection/lib/net462/System.Reflection.dll"
#r "../packages/Microsoft.Win32.Registry/lib/net461/Microsoft.Win32.Registry.dll"
#r "../packages/System.Globalization.Calendars/lib/net46/System.Globalization.Calendars.dll"
#r "../packages/System.IO/lib/net462/System.IO.dll"
#r "../packages/System.Threading.Tasks.Extensions/lib/netstandard2.0/System.Threading.Tasks.Extensions.dll"
#r "../packages/System.Threading.ThreadPool/lib/net46/System.Threading.ThreadPool.dll"
#r "../packages/Microsoft.Win32.Primitives/lib/net46/Microsoft.Win32.Primitives.dll"
#r "../packages/System.AppContext/lib/net463/System.AppContext.dll"
#r "../packages/System.Diagnostics.Tracing/lib/net462/System.Diagnostics.Tracing.dll"
#r "../packages/System.IO.FileSystem.Primitives/lib/net46/System.IO.FileSystem.Primitives.dll"
#r "../packages/System.Runtime.Extensions/lib/net462/System.Runtime.Extensions.dll"
#r "../packages/System.Security.AccessControl/lib/net461/System.Security.AccessControl.dll"
#r "../packages/System.Threading.Thread/lib/net46/System.Threading.Thread.dll"
#r "../packages/Persimmon/lib/net45/Persimmon.dll"
#r "../packages/System.Runtime/lib/net462/System.Runtime.dll"
#r "../packages/System.Security.Principal.Windows/lib/net461/System.Security.Principal.Windows.dll"
#r "../packages/System.Reflection.TypeExtensions/lib/net461/System.Reflection.TypeExtensions.dll"
#r "../packages/FSharp.Compiler.Service/lib/net45/FSharp.Compiler.Service.dll"
#r "../packages/Microsoft.DiaSymReader.PortablePdb/lib/net45/Microsoft.DiaSymReader.PortablePdb.dll"
#r "../packages/System.Reflection.Metadata/lib/netstandard2.0/System.Reflection.Metadata.dll"
#r "../packages/Persimmon.Runner/lib/net40/Persimmon.Runner.dll"
#r "../packages/Persimmon.Script/lib/net45/Persimmon.Script.dll"
#r "System"
#r "System.ComponentModel.Composition"
#r "System.Core"
#r "System.Runtime.Serialization"
#r "System.Numerics"
#r "System.Xml"
#r "System.IO.Compression"
#r "System.Xml.Linq"
#r "System.IO.Compression.FileSystem"
#r "System.Net.Http"
#r "Microsoft.CSharp"
// #r "ISymWrapper, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
// #r "System.IO, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
// #r "System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
open Newtonsoft.Json
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