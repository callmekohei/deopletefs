[![MIT-LICENSE](http://img.shields.io/badge/license-MIT-blue.svg?style=flat)](https://github.com/callmekohei/deopletefs/blob/master/LICENSE)
[![Build Status](https://travis-ci.org/callmekohei/deopletefs.svg?branch=master)](https://travis-ci.org/callmekohei/deopletefs)


# deopletefs

`deopletefs` is Engine for [deoplete-fsharp](https://github.com/callmekohei/deoplete-fsharp)  
`deopletefs` is a command-line interface to the FSharp.Compiler.Service.  
`deopletefs` provides only [Getting auto-complete lists](https://fsharp.github.io/FSharp.Compiler.Service/editor.html#Getting-auto-complete-lists)


## Installing

```
$ git clone deapth -1 https://github.com/callmekohei/deopletefs
```

## Build and Test

Requires bash, mono and FSharp installed

```
$ bash build.bash
$ fsharpi ./src/test.fsx
```

## Communication protocol

input  : Json  
output : base64String

```text
// Example of Json
{
    "Row"      : 1
  , "Col"      : 5
  , "Line"     : "List."
  , "FilePath" : "./foo.fsx"
  , "Source"   : "List."
  , "Init"     : "false"
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
// meanig: {"word":"finish initialize!","info":[[""]]}
---> eyJ3b3JkIjoiZmluaXNoIGluaXRpYWxpemUhIiwiaW5mbyI6W1siIl1dfQ==
```

complete
```text
// input as Json
{ "Row" : 1 ,"Col": 5, "Line": "List.", "FilePath" : "./dummy.fsx", "Source" : "", "Init":"false"}

// return as base64String
---> eyJ3b3JkIjoiYWxsUGFpcnMiLCJpbmZvIjpb....
```


quit
```
quit
```

## License

MIT License
