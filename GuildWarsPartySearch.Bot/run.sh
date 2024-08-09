#!/bin/bash

NOW=$(date '+%Y-%m-%d_%H-%M-%S')

BIN_DIR=$PWD/linuxbuild/bin;

# Absolute path to client executable
CLIENT_EXE=$BIN_DIR/client;
# Absolute path to log file
LOG_FILE=$PWD/logs/$NOW.txt;
# Contains the build version number of guild wars
BUILD_VERSION_FILE=$PWD/Gw.build;
# Absolute location of plugin on disk
PLUGIN_EXE=$BIN_DIR/libGuildWarsPartySearch.Bot.so;

# TODO: ENV vars
RUN_EMAIL="${EMAIL:-null@arenanet.com}";
RUN_PASSWORD="${PASSWORD:-nopassword}";
RUN_CHARACTER="${CHARACTER:-No Character}";
RUN_MAP_ID="${MAP_ID:-0}";
RUN_DISTRICT="${DISTRICT:-0}";

# Absolute log
LOG_DIR=$(dirname "$LOG_FILE")
rm -R $LOG_DIR;
mkdir -p $LOG_DIR;
touch $LOG_FILE;

"$CLIENT_EXE" -email "$RUN_EMAIL" -character "$RUN_CHARACTER" -district "$RUN_DISTRICT" -mapid "$RUN_MAP_ID" -password "$RUN_PASSWORD" -l "$LOG_FILE" -file-game-version "$BUILD_VERSION_FILE" "$PLUGIN_EXE" 