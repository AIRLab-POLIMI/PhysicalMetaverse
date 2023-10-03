#!/bin/bash

# kill current MAIN.PY
kill -9 main.py

# start new MAIN.PY
cd "$(dirname "$(readlink -f "$0")")"
python3 main.py