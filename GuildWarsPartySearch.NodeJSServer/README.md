# GuildWarsPartySearch.NodeJSServer

## Local development

1. Copy `config.example.json` to `config.json`, and edit it for the deployment.
2. `npm install`
3. `npm run dev`

## Running the docker container

1. Copy `config.example.json` to `config.json`, and edit it for the deployment.
2. `docker build -t partysearchserver_alpine .`
3. *Windows:* `docker run -d --restart always --name partysearchserver -p8080:80 -v "%cd%":/app partysearchserver_alpine ./run.sh`
4. *Linux:* `docker run -d --restart always --name "${PWD##*/}" -p8080:80 --replace -v "$PWD":/app:Z partysearchserver_alpine ./run.sh`

## Redeploying

Run `./redeploy.sh` from this directory.
