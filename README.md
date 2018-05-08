[![MIT-LICENSE](http://img.shields.io/badge/license-MIT-blue.svg?style=flat)](https://github.com/callmekohei/deopletefs/blob/master/LICENSE)
[![Build Status](https://travis-ci.org/callmekohei/deopletefs.svg?branch=master)](https://travis-ci.org/callmekohei/deopletefs)


# deopletefs


Engine for [deoplete-fsharp](https://github.com/callmekohei/deoplete-fsharp)





## Installing

```
$ git clone deapth -1 https://github.com/callmekohei/deopletefs
```

## Compile
```
$ bash build.bash
```

## Code test
```
$ fsharpi ./src/test.fsx
```



## Check
```shell
$ cd bin/
$ mono deopletefs.exe
```
first initialize
```json
{ "Row" : -9999 ,"Col": -9999, "Line": "", "FilePath" : "../test/dummy.fsx", "Source" : "", "Init":"dummy_init"}
```
return
```json
{"word":"finish initialize!","info":[[""]]}
```
second initialize
```json
{ "Row" : 1 ,"Col": 5, "Line": "List.", "FilePath" : "../test/dummy.fsx", "Source" : "", "Init":"false"}
```
return
```
{"word":"allPairs"  , "info":[["val allPairs : list1:'T1 list -> list2:'T2 list -> ('T1 * 'T2) list"]]}
{"word":"append"    , "info":[["val append : list1:'T list -> list2:'T list -> 'T list"]]}
{"word":"average"   , "info":[["val average : list:'T list -> 'T (requires member ( + ) and member DivideByInt and member get_Zero)"]]}
{"word":"averageBy" , "info":[["val averageBy : projection:('T -> 'U) -> list:'T list -> 'U (requires member ( + ) and member DivideByInt and member get_Zero)"]]}
{"word":"choose"    , "info":[["val choose : chooser:('T -> 'U option) -> list:'T list -> 'U list"]]}
...
```
Quit deopletefs
```
quit
``` 
