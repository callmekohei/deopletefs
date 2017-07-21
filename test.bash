# ===========================================================================
#  FILE    : easyCompile.bash
#  AUTHOR  : callmekohei <callmekohei at gmail.com>
#  License : MIT license
# ===========================================================================

SCRIPT_DIR=$(cd $(dirname $0);pwd)

cp -rf $SCRIPT_DIR'/packages/Persimmon.Console/tools' ./test/
cp -rf ./bin_deopletefs ./test/tools/
cp -f  ./test/Persimmon.Console.exe.config ./test/tools/

fsharpc -a ./test/test.fsx
mono ./test/tools/Persimmon.Console.exe ./test/test.dll
