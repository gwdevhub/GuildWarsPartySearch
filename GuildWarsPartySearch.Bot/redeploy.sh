#!/bin/bash

# Stops a container matching the name if this directory, redeploys latest code from repo, starts container again

BOT_FOLDER=$PWD
BOT_NAME=${PWD##*/}

echo $BOT_NAME;

docker stop $BOT_NAME;
# Clear any existing tmp folder
rm -R /tmp$BOT_FOLDER;

# Clone into tmp folder
mkdir -p /tmp$BOT_FOLDER;
cd /tmp$BOT_FOLDER;
git clone --recursive https://github.com/gwdevhub/GuildWarsPartySearch.git

# Copy new bot code back into original folder
cp -ura ./GuildWarsPartySearch/GuildWarsPartySearch.Bot/* $BOT_FOLDER;
rm -R $BOT_FOLDER/linuxbuild;
chmod -R 777 $BOT_FOLDER;

# Clear any existing tmp folder
rm -R /tmp$BOT_FOLDER;

docker run -d --restart always --name $BOT_NAME -v "$BOT_FOLDER":/app:Z --replace partysearchbot_alpine ./run.sh