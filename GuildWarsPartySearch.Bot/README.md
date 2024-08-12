# GuildWarsPartySearch.Bot

## Running the docker container

1. Copy config.example.sh to config.sh, and edit it to add your Guild Wars credentials
2. `docker build -t partysearchbot_alpine .`
3. *Windows:* `docker run -d --restart always --name partysearchbot -v "%cd%":/app partysearchbot_alpine ./run.sh`
3. *Linux:* `docker run -d --restart always --name partysearchbot -v "$PWD":/app partysearchbot_alpine:Z ./run.sh`

## If the source code for the bot has been changed

1. `docker stop partysearchbot`
2. `rm -R linuxbuild`
3. `docker start partysearchbot`

## Debugging

Segmentation fault in docker usually means the bot failed; running manually by sshing into the server could help give more info in the console.
1. `docker rm -f partysearchbot`
2. `docker run -d --restart always --name partysearchbot -v "%cd%":/app partysearchbot_alpine`
3. `docker exec -ti partysearchbot bash`
4. `./run.sh` - this should fail with a useless segmentation fault message. Copy the command shown in the output.
5. `gdb --args <your_copied_text>` - this should open a gdb prompt for the next steps.
6. `run` - this should end up printing out `Program received signal SIGSEGV, Segmentation fault.`
6. `bt` to view the call stack, explaining the error so you can actually do something about it!
