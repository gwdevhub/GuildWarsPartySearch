dotnet publish -c Release
docker build -t guildwarspartysearch.server .
docker tag guildwarspartysearch.server guildwarspartysearch.azurecr.io/guildwarspartysearch.server