# GuildWarsPartySearch.Bot

## Running the docker container

1. Copy config.example.sh to config.sh, and edit it to add your Guild Wars credentials
2. `docker build -t partysearchserver_alpine .`
3. *Windows:* `docker run -d --restart always --name partysearchserver -p80:80 -v "%cd%":/app partysearchserver_alpine ./run.sh`
3. *Linux:* `docker run -d --restart always --name "${PWD##*/}" -p80:80 --replace -v "$PWD":/app:Z partysearchserver_alpine ./run.sh`

## If the source code for the bot has been changed

1. `docker stop "${PWD##*/}"`
2. `rm -R linuxbuild`
3. `docker start "${PWD##*/}"`
