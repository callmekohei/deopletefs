[![MIT-LICENSE](http://img.shields.io/badge/license-MIT-blue.svg?style=flat)](https://github.com/callmekohei/deopletefs/blob/master/LICENSE)
[![Build Status](https://travis-ci.org/callmekohei/deopletefs.svg?branch=master)](https://travis-ci.org/callmekohei/deopletefs)


# deopletefs

`deopletefs` is Engine for [deoplete-fsharp](https://github.com/callmekohei/deoplete-fsharp)  
`deopletefs` is a command-line interface to the FSharp.Compiler.Service.  
`deopletefs` provides only [Getting auto-complete lists](https://fsharp.github.io/FSharp.Compiler.Service/editor.html#Getting-auto-complete-lists)

## Requires
[mono](https://github.com/mono/mono)  (>= Mono 5.4.0 )  
[fsharp](https://github.com/fsharp/fsharp)

## Install, build and test

```
$ git clone deapth -1 https://github.com/callmekohei/deopletefs
$ cd ./deopletefs/
$ bash build.bash
$ fsharpi ./src/test.fsx
```

## Communication protocol

input  : Json  
output : base64String ( innner data is Json )

Example of input
```text
{
    "Row"      : 1
  , "Col"      : 5
  , "Line"     : "List."
  , "FilePath" : "./foo.fsx"
  , "Source"   : "List."
  , "Init"     : "false"
}
```

Example of output
```text
// return as base64String
---> eyJ3b3JkIjoiYWxsUGFpcnMiLCJpbmZvIjpb....

// decode64
{
  "word": "allPairs",
  "info": [
    [
      "val allPairs : list1:'T1 list -> list2:'T2 list -> ('T1 * 'T2) list"
    ]
  ]
}
```

## Usage

launch
```shell
$ cd bin/
$ mono deopletefs.exe
```

initialize
```text
// input as Json
{ "Row" : -9999 ,"Col": 1, "Line": "", "FilePath" : "./dummy.fsx", "Source" : "", "Init":"true"}

// return as base64String
---> eyJ3b3JkIjoiZmluaXNoIGluaXRpYWxpemUhIiwiaW5mbyI6W1siIl1dfQ==

// decode64
{"word":"finish initialize!","info":[[""]]}
```

complete list
```text
// input as Json
{ "Row" : 1 ,"Col": 5, "Line": "List.", "FilePath" : "./dummy.fsx", "Source" : "", "Init":"false"}

// return as base64String
---> eyJ3b3JkIjoiYWxsUGFpcnMiLCJpbmZvIjpb....

// decode64
{"word":"allPairs","info":[["val allPairs : list1:'T1 list -> list2:'T2 list -> ('T1 * 'T2) list"]]}
{"word":"append","info":[["val append : list1:'T list -> list2:'T list -> 'T list"]]}
{"word":"average","info":[["val average : list:'T list -> 'T (requires member ( + ) and member DivideByInt and member get_Zero)"]]}
{"word":"averageBy","info":[["val averageBy : projection:('T -> 'U) -> list:'T list -> 'U (requires member ( + ) and member DivideByInt and member get_Zero)"]]}
...
```

quit
```
quit
```
