#!/bin/bash

python3 ./Dependencies/Headquarter/tools/update_binary.py
python3 ./Dependencies/Headquarter/tools/convert_key_to_text.py
mkdir -p ./data;
cp -ura ./Dependencies/Headquarter/data/* ./data
cp -ura ./Dependencies/Headquarter/Gw.build Gw.build