#r "./packages/FSharp.Core/lib/net45/FSharp.Core.dll"
printfn "%A" <| System.Reflection.Assembly.GetAssembly( typeof<list<int>> ).FullName
