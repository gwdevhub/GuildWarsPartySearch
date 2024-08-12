#!/bin/bash

#----------------
# Modify this file and save it as "config.sh" before running (or restarting) the docker container
#----------------

# Email address for your Guild Wars account
EMAIL=""
# Password for your Guild Wars account
PASSWORD=""
# Character name to travel to listen for party searches
CHARACTER=""
# Map ID to travel to listen for party searches, default embark beach
MAP_ID="857"
# Composite district enum to use (e.g. 1 = AMERICAN) - check source code!
DISTRICT="1"
# API Key, necessary
WEBSOCKET_API_KEY="development"
# Endpoint to send party searches to
WEBSOCKET_URL="ws://localhost/party-search/update"
