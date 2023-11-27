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
                            Description = "WebSocket endpoint for live feed.",
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
                            Description = $"WebSocket endpoint for posting party search updates. Protected by {ApiKeyProtected.ApiKeyHeader} header.",
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
