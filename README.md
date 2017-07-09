
[![Build Status](https://travis-ci.org/callmekohei/deopletefs.svg?branch=master)](https://travis-ci.org/callmekohei/deopletefs)


# deopletefs


Engine for [deoplete-fsharp](https://github.com/callmekohei/deoplete-fsharp)





## Installing

```
$ git clone https://github.com/callmekohei/deopletefs
```

## How to compile
```shell
$ bash build.bash
```
result
```shell
$ ls bin_deopletefs/
FSharp.Compiler.Service.dll deopletefs.exe
FSharp.Core.dll             log.txt
Newtonsoft.Json.dll
```


## Check
```shell
$ cd bin_deopletefs/
$ mono deopletefs.exe 
```
input
```json
{ "Row" : -9999 ,"Col": -9999, "Line": "", "FilePath" : "../test/abc.fsx", "Source" : "", "Init":"dummy_init"}
```
return
```json
{"word":"finish initialize!","info":[[""]]}
```
input
```json
{ "Row" : 1 ,"Col": 5, "Line": "List.", "FilePath" : "../test/abc.fsx", "Source" : "", "Init":"false"}
```
return
```json
{"word":"allPairs"  , "info":[["val allPairs : list1:'T1 list -> list2:'T2 list -> ('T1 * 'T2) list"]]}
{"word":"append"    , "info":[["val append : list1:'T list -> list2:'T list -> 'T list"]]}
{"word":"average"   , "info":[["val average : list:'T list -> 'T (requires member ( + ) and member DivideByInt and member get_Zero)"]]}
{"word":"averageBy" , "info":[["val averageBy : projection:('T -> 'U) -> list:'T list -> 'U (requires member ( + ) and member DivideByInt and member get_Zero)"]]}
{"word":"choose"    , "info":[["val choose : chooser:('T -> 'U option) -> list:'T list -> 'U list"]]}
...
```


## LICENCE  

The MIT License (MIT)
