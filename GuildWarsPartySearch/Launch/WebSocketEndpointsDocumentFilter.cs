using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GuildWarsPartySearch.Server.Launch;

public sealed class WebSocketEndpointsDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Paths.Add("/party-search/live-feed",
            new OpenApiPathItem
            { 
                Summary = "Connect to WebSocket for live feed",
                Description = "WebSocket endpoint for live feed.",
                Operations = new Dictionary<OperationType, OpenApiOperation>
                {
                    { 
                        OperationType.Get,
                        new OpenApiOperation()
                        { 
                            Tags = new List<OpenApiTag>
                            { 
                                new() 
                                { 
                                    Name = "PartySearch" 
                                } 
                            }, 
                            Description = @"
WebSocket endpoint for live feed.

Sends periodic party search updates. Example:
```json
{
    ""Searches"":
    [
        {
            ""map_id"":857,
            ""district_region"":-2,
            ""district_number"":1,
            ""district_language"":0,
            ""parties"":
            [
                {
                    ""party_id"":1,
                    ""district_number"":1,
                    ""district_language"":0,
                    ""message"":"""",
                    ""sender"":""Alucard Dracula"",
                    ""party_size"":4,
                    ""hero_count"":3,
                    ""hard_mode"":1,
                    ""search_type"":1,
                    ""primary"":3,
                    ""secondary"":10,
                    ""level"":20
                }
            ]
        }
    ]
}
```",
                            Responses = new OpenApiResponses
                            {
                                { "200", new OpenApiResponse(){ Description = "Success" } },
                            }
                        }
                    }
                },
                Parameters = new List<OpenApiParameter>
                {
                    new()
                    {
                        Name = "Connection",
                        In = ParameterLocation.Header,
                        Description = "WebSocket upgrade header",
                        Required = true,
                       
                    },
                    new()
                    {
                        Name = "Sec-WebSocket-Version",
                        In = ParameterLocation.Header,
                        Description = "WebSocket protocol version",
                        Required = true
                    },
                    new()
                    {
                        Name = "Sec-WebSocket-Key",
                        In = ParameterLocation.Header,
                        Description = "WebSocket key",
                        Required = true
                    },
                }
            });
        swaggerDoc.Paths.Add("/party-search/update",
            new OpenApiPathItem
            {
                Summary = "Connect to WebSocket for updates",
                Description = $"WebSocket endpoint for posting party search updates. Protected by *IP whitelisting*. Requires User-Agent header to be set",
                Operations = new Dictionary<OperationType, OpenApiOperation>
                {
                    {
                        OperationType.Get,
                        new OpenApiOperation()
                        { 
                            Tags = new List<OpenApiTag>
                            { 
                                new()
                                { 
                                    Name = "PartySearch" 
                                } 
                            }, 
                            Description = @$"
WebSocket endpoint for posting party search updates.

Protected by *IP whitelisting*.

Requires *User-Agent* header to be set.

Accepts json payloads. Example:
```json
{{
  ""map_id"": 857,
  ""district_region"": -2,
  ""district_number"": 1,
  ""district_language"": 0,
  ""parties"": [
    {{
      ""party_id"": 1,
      ""district_number"": 1,
      ""district_language"": 0,
      ""message"": """",
      ""sender"": ""Demia Frelluis"",
      ""party_size"": 4,
      ""hero_count"": 3,
      ""hard_mode"": 1,
      ""search_type"": 1,
      ""primary"": 3,
      ""secondary"": 10,
      ""level"": 20
    }}
  ]
}}
```

Returns json payloads. Example:
```json
{{
    ""Result"": 0,
    ""Description"": ""Success""
}}
```",
                            Responses = new OpenApiResponses
                            {
                                { "200", new OpenApiResponse(){ Description = "Success" } },
                                { "403", new OpenApiResponse(){ Description = "Forbidden" } }
                            }
                        }
                    }
                },
                Parameters = new List<OpenApiParameter>
                {
                    new()
                    {
                        Name = "Connection",
                        In = ParameterLocation.Header,
                        Description = "WebSocket upgrade header",
                        Required = true,

                    },
                    new()
                    {
                        Name = "Sec-WebSocket-Version",
                        In = ParameterLocation.Header,
                        Description = "WebSocket protocol version",
                        Required = true
                    },
                    new()
                    {
                        Name = "Sec-WebSocket-Key",
                        In = ParameterLocation.Header,
                        Description = "WebSocket key",
                        Required = true
                    },
                },
            });
    }
}
