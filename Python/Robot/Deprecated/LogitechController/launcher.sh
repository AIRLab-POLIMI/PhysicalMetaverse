#!/bin/sh
# launcher.sh
# navigate to home directory, then to this directory, then execute python script, then back home

# start new MAIN.PY
#cd "$(dirname "$(readlink -f "$0")")"
python3 main.py
