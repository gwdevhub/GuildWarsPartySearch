dotnet publish -r linux-x64 -c Release -o Publish/
Copy-Item -Path Config.Release.json -Destination Publish/Config.json
docker build -t guildwarspartysearch.server .
docker tag guildwarspartysearch.server guildwarspartysearch.azurecr.io/guildwarspartysearch.server