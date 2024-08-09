#!/bin/bash
dpkg -s python3 > /dev/null 2>&1 || (
  apt install python3 pip -y;
  pip install tqdm;
  pip install pefile;
);
python3 ./Dependencies/Headquarter/tools/update_binary.py;
mkdir -p ./data;
cp -ura ./Dependencies/Headquarter/data/* ./data;
cp -ura ./Dependencies/Headquarter/Gw.build Gw.build;