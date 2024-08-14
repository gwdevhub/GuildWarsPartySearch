#!/bin/bash

NOW=$(date '+%Y-%m-%d_%H-%M-%S')

BIN_DIR=$PWD/linuxbuild/bin;

# Load in the login credentials from config.sh
source config.sh

# Absolute path to client executable
CLIENT_EXE=$BIN_DIR/client;
# Absolute path to log file
LOG_FILE="/var/log/partysearchbot/$CHARACTER/$NOW.txt";
# Contains the build version number of guild wars
BUILD_VERSION_FILE=$PWD/Gw.build;
# Absolute location of plugin on disk
PLUGIN_EXE=$BIN_DIR/libGuildWarsPartySearch.Bot.so;

# Absolute log
LOG_DIR=$(dirname "$LOG_FILE")
rm -R "$LOG_DIR";
mkdir -p "$LOG_DIR";
touch "$LOG_FILE";


# Check with the GW fileserver for an updated gw version, and copy it into Gw.build
source check_and_update_gw_keys.sh

# Build the solution if its not already built
if [ ! -f $CLIENT_EXE ]; then
source build.sh
fi

export HEADQUARTER_PASSWORD="$PASSWORD"
export HEADQUARTER_2FA_SECRET="$TOTP_SECRET"

RUN_CMD=$(cat <<EOF
"$CLIENT_EXE" \
-email "$EMAIL" \
-character "$CHARACTER" \
-district "$DISTRICT" \
-travel-mapid "$MAP_ID" \
-api-key "$WEBSOCKET_API_KEY" \
-websocket-url "$WEBSOCKET_URL" \
-l "$LOG_FILE" \
-file-game-version "$BUILD_VERSION_FILE" \
"$PLUGIN_EXE"
EOF
)


echo "
------- Command to run:
";
echo $RUN_CMD
echo "
-------
";


bash -c "$RUN_CMD"
