#!/usr/bin/env bash

foo=$( fsharpi --noframework ./src/test.fsx )
arr=($foo)
bar=$(echo ${arr[0]} | grep 'x')

# -n is nonZero length
if [ -n "$bar" ]; then
  echo 'deopletefs test result is error'
  exit 1
else
  echo 'deopletefs test result is ok'
  exit 0
fi
