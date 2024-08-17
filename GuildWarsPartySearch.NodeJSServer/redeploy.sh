#!/bin/bash

# Stops a container matching the name if this directory, redeploys latest code from repo, starts container again

BOT_FOLDER=$PWD
BOT_NAME=${PWD##*/}

echo $BOT_NAME;
echo $BOT_FOLDER;

docker stop -f "$BOT_NAME";
docker rm -f "$BOT_NAME";
# Clear any existing tmp folder
rm -R "/tmp$BOT_FOLDER";

# Clone into tmp folder
mkdir -p "/tmp$BOT_FOLDER";
cd "/tmp$BOT_FOLDER";
chmod 777 .;
git clone --recursive https://github.com/gwdevhub/GuildWarsPartySearch.git

# Copy new bot code back into original folder
cp -ura ./GuildWarsPartySearch/GuildWarsPartySearch.NodeJSServer/* "$BOT_FOLDER";
chmod -R 777 "$BOT_FOLDER";

# Clear any existing tmp folder
rm -R "/tmp$BOT_FOLDER";

cd "$BOT_FOLDER";

RUN_CMD=$(cat <<EOF
docker run -d --restart always --name "$BOT_NAME" -p8080:80 -v "$BOT_FOLDER":/app:Z --replace partysearchserver_alpine ./run.sh
EOF
)
echo $RUN_CMD;

bash -c "$RUN_CMD"
