# ===========================================================================
#  FILE    : easyCompile.bash
#  AUTHOR  : callmekohei <callmekohei at gmail.com>
#  License : MIT license
# ===========================================================================


# Please fit your path if you need.
mono=''
fsc=''

mono .paket/paket.bootstrapper.exe
mono .paket/paket.exe install

SCRIPT_PATH='./deopletefs.fsx'
SCRIPT_DIR=$(cd $(dirname $0);pwd)

# Create dummyJson.fsx
fsharpi ./util/create_dummyJson.fsx > ./util/dummyJson.fsx


# Create EXE file
declare -a arr=(
   '--nologo'
   '--simpleresolution'
   # '--define:DEBUG'
    $SCRIPT_PATH
)

if [ "$mono" = "" ] || [ "$fsc" = "" ] ; then
    fsharpc ${arr[@]}
else
    $mono $fsc --exename:$(basename "$0") ${arr[@]}
fi


# Create bin folder
if [ -e ./bin_deopletefs ] ; then
    rm -rf ./bin_deopletefs
fi

mkdir ./bin_deopletefs


# EXE file -> ./bin folder (move)
EXE_PATH=${SCRIPT_PATH%.fsx}.exe
mv $EXE_PATH ./bin_deopletefs


# Get path of DLL from FSharp script
cat $SCRIPT_PATH \
    | grep --color=never -e '^#r' \
    | sed  -e 's/^.*@"//g' \
           -e 's/"$//g' \
           -e 's%^\.%'$SCRIPT_DIR'%g' \
    > /tmp/std.out.$$ 2>/tmp/std.err.$$

std_out=`cat /tmp/std.out.$$`
std_err=`cat /tmp/std.err.$$`
rm -f /tmp/std.out.$$
rm -f /tmp/std.err.$$


# DLL files -> ./bin folder (copy)
if [ -n "$std_out" ] ; then

    cp $SCRIPT_DIR'/packages/FSharp.Core/lib/net45/FSharp.Core.dll' ./bin_deopletefs/

    Arr_DLL_PATH=($(echo $std_out))
    len=$(( ${#Arr_DLL_PATH[@]} - 1 ))

    for i in $(seq 0 $len) ; do
        cp ${Arr_DLL_PATH[$i]} ./bin_deopletefs/
    done

fi

# add log.txt
touch ./bin_deopletefs/log.txt

# add perssimon
cp -rf $SCRIPT_DIR'/packages/Persimmon.Console/tools' ./
