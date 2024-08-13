#!/bin/bash

#----------------
# Modify this file and save it as "config.sh" before running (or restarting) the docker container
#----------------

# Email address for your Guild Wars account
EMAIL=""
# Password for your Guild Wars account
PASSWORD=""
# Secret used for an Authenticator App to verify 2FA with this account. 16 chars.
2FA_SECRET="" 
# Character name to travel to listen for party searches
CHARACTER=""
# Map ID to travel to listen for party searches, default embark beach
MAP_ID="857"
# Composite district enum to use (e.g. 1 = AMERICAN) - check Dependencies/Headquarter/include/client/constants.h!
DISTRICT="1"
# Endpoint to send party searches to
WEBSOCKET_URL="wss://localhost"
# Api key to provide as a header when connecting to above websocket
WEBSOCKET_API_KEY="development"
