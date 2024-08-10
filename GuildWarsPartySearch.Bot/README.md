# GuildWarsPartySearch.Bot

## Running the docker container

1. Copy config.example.sh to config.sh, and edit it to add your Guild Wars credentials
2. `docker build -t partysearchbot_alpine .`
3. *Windows:* `docker run -d --restart always --name partysearchbot -v "%cd%":/app partysearchbot_alpine ./run.sh`
3. *Linux:* `docker run -d --restart always --name partysearchbot -v "$pwd":/app partysearchbot_alpine ./run.sh`

## If the source code for the bot has been changed

1. `docker stop partysearchbot`
2. `rm -R linuxbuild`
3. `docker start partysearchbot`

## Debugging

Segmentation fault in docker usually means the bot failed; running manually by sshing into the server could help give more info in the console.

1. `docker run -d --restart always --name partysearchbot -v "%cd%":/app partysearchbot_alpine`
2. `docker exec -ti partysearchbot bash`
3. `./run.sh`
