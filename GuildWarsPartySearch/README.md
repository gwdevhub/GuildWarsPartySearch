# GuildWarsPartySeach.Server

## Building

### Local
- Install [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Restore project
- Build and Launch Debug configuration

### Docker
#### Build
- `docker build -t guildwarspartysearch.server .`
- `docker create --name partysearch -p 8080:80 guildwarspartysearch.server.latest`
  - For a custom configuration: `docker cp [PATH_TO_CUSTOM_CONFIG].json partysearch:/app/Config.json`
- `docker start partysearch`

#### Change configuration
- `docker stop partysearch`
- `docker cp [PATH_TO_CUSTOM_CONFIG].json partysearch:/app/Config.json`
- `docker start partysearch`

#### Change static content
- `docker stop partysearch`
- `docker cp [PATH_TO_CONTENT_DIR] partysearch:/app`
- `docker start partysearch`

#### Manually modify SQLite db
- `docker stop partysearch`
- `docker cp partysearch:/app/partysearches.db [LOCAL_PATH_FOR_DB]/partysearches.db`
- Use something like [DB4S](https://sqlitebrowser.org/) to browse and modify the database file
- `docker cp [LOCAL_PATH_FOR_DB]/partysearches.db partysearch:/app/partysearches.db`
- `docker start partysearch`

## Configuration
Found in Config.[ENV].json
- `ServerOptions.Port` dictates the port on which the server will accept connections
- `PartySearchDatabaseOptions.TableName` is the name of the party searches table in the SQLite db
- `BotHistoryDatabaseOptions.TableName` is the name of the bot actions history table in the SQLite db
- `IpWhitelistOptions.Addresses` is an array of addresses that are whitelisted by IP. These are the addresses that are allowed to open bot connections and push updates
- `ContentOptions.StagingFolder` is the relative path of the static content that is served by the server