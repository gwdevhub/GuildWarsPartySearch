dotnet publish -c Release
Copy-Item -Path Config.Release.json -Destination ./bin/Release/net8.0/publish/Config.json
docker build -t guildwarspartysearch.server .
docker tag guildwarspartysearch.server guildwarspartysearch.azurecr.io/guildwarspartysearch.server