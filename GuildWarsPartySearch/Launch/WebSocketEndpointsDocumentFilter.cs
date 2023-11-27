using GuildWarsPartySearch.Server.Filters;
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
            ""Campaign"":""Prophecies"",
            ""Continent"":""Tyria"",
            ""Region"":""Kryta"",
            ""Map"":""Lions Arch"",
            ""District"":""En - 1"",
            ""PartySearchEntries"":
            [
                {
                    ""CharName"":""Shiro Tagachi"",
                    ""PartySize"":8,
                    ""PartyMaxSize"":8,
                    ""Npcs"":0
                },
                {
                    ""CharName"":""Kormir Sucks"",
                    ""PartySize"":8,
                    ""PartyMaxSize"":8,
                    ""Npcs"":0
                }
            ]
        },
        {
            ""Campaign"":""Prophecies"",
            ""Continent"":""Tyria"",
            ""Region"":""Kryta"",
            ""Map"":""Warrior's Isle"",
            ""District"":""En - 1"",
            ""PartySearchEntries"":
            [
                {
                    ""CharName"":""Don Abaddon"",
                    ""PartySize"":0,
                    ""PartyMaxSize"":16,
                    ""Npcs"":2
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
                Description = $"WebSocket endpoint for posting party search updates. Protected by {ApiKeyProtected.ApiKeyHeader} header.",
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

Protected by *{ApiKeyProtected.ApiKeyHeader}* header.

Accepts json payloads. Example:
```json
{{
    ""Continent"": ""Tyria"",
    ""Campaign"": ""Prophecies"",
    ""Region"": ""Kryta"",
    ""Map"": 4,
    ""District"": ""En - 1"",
    ""PartySearchEntries"": [
        {{
            ""PartySize"": 8,
            ""PartyMaxSize"": 8,
            ""Npcs"": 0,
            ""CharName"": ""Shiro Tagachi"",
        }},
        {{
            ""PartySize"": 8,
            ""PartyMaxSize"": 8,
            ""Npcs"": 0,
            ""CharName"": ""Kormir Sucks"",
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
                        Name = ApiKeyProtected.ApiKeyHeader,
                        In = ParameterLocation.Header,
                        Description = "Header is required in order to connect to this endpoint",
                        Required = true
                    },
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
