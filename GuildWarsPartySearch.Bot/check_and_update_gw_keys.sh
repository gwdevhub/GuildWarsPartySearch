#!/bin/bash

python3 ./Dependencies/Headquarter/tools/update_binary.py;
mkdir -p ./data;
cp -ura ./Dependencies/Headquarter/data/* ./data;
cp -ura ./Dependencies/Headquarter/Gw.build Gw.build;