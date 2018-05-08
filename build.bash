# ===========================================================================
#  FILE    : build.bash
#  AUTHOR  : callmekohei <callmekohei at gmail.com>
#  License : MIT license
# ===========================================================================

#!/bin/bash

# fit your file path
FSX_PATH=./src/deopletefs.fsx
Lib_PATH=./.paket/load/net471/main.group.fsx


install_lib() (
    if [ ! -e ./packages ] ; then
        paket install
    fi
)

create_exe_file() (
    declare -a local arr=(
        $FSX_PATH
        --nologo
        --simpleresolution
        --out:./bin/$(basename $FSX_PATH .fsx).exe
    )
    fsharpc ${arr[@]}
)

arrange_text() {
    local line
    while read -r line
    do
        echo "$line" \
        | sed -e 's/#r //g' \
              -e 's/"//g'   \
        | grep --color=never -e "^\." \
        | sed -e 's|^.*packages|\./packages|g'
    done
}

copy_dll_to_bin_folder() {
    local line
    while read -r line
    do
        cp $line ./bin/
    done
}


if [ -e ./bin ] ; then
    echo 'do nothing!'
else
    mkdir ./bin
    install_lib
    fsharpi ./src/create_dummyJson.fsx > ./src/dummyJson.fsx
    create_exe_file
    cat $Lib_PATH | arrange_text | copy_dll_to_bin_folder
    # add log.txt
    touch ./bin_deopletefs/log.txt
fi





