# GuildWarsPartySearch
Party Search Aggregator

## GuildWarsPartySearch.Server

### Usage
- Bots connect with websocket to `party-search/update` where they push the current state of their map. Sample payload:
```json
{
  "map_id": 857,
  "district_region": -2,
  "district_number": 1,
  "district_language": 0,
  "parties": [
    {
      "party_id": 1,
      "district_number": 1,
      "district_language": 0,
      "message": "",
      "sender": "Gaile Gray",
      "party_size": 4,
      "hero_count": 3,
      "hard_mode": 1,
      "search_type": 1,
      "primary": 3,
      "secondary": 10,
      "level": 20
    }
  ]
}
```
- Bots need to have `User-Agent` header set and have their IP whitelisted. `User-Agent` identifies the bot.
- The client/user connects with websocket to `party-search/live-feed`
- When the client first connects, it receives a json with all the party searches currently in the database (all the snapshots). Sample payload:
```json
{
    "Searches":
    [
        {
            "map_id":857,
            "district_region":-2,
            "district_number":1,
            "district_language":0,
            "parties":
            [
                {
                    "party_id":1,
                    "district_number":1,
                    "district_language":0,
                    "message":"",
                    "sender":"Gaile Gray",
                    "party_size":4,
                    "hero_count":3,
                    "hard_mode":1,
                    "search_type":1,
                    "primary":3,
                    "secondary":10,
                    "level":20
                }
            ]
        }
    ]
}
```
- Whenever a bot posts an update, all clients receive that update in the same json format as the first message
- There's a status page `/status/bots` where you can see a list of all the currently connected bots. You need to have your IP whitelisted set to see the page
- Check https://guildwarspartysearch.azurewebsites.net/swagger for the swagger docs and examples of the server

## GuildWarsPartySearch.BotUploader
The resulting dll needs to be injected into a running Guild Wars process.

When injecting, the dll needs a file called config.txt in the same folder.

### Config.txt
Config file that contains 3 lines.
1. Uri of the party service (guildwarspartysearch.northeurope.azurecontainer.io)
2. The API Key necesarry for the bot to access the upload endpoint
3. Bot name. This is an identifier of the bot. This appears on /status/bots when the bot is connected
