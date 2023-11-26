using GuildWarsPartySearch.Server.Endpoints;
using System.Core.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Extensions;
using System.Net.WebSockets;

namespace GuildWarsPartySearch.Server.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication MapWebSocket<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TWebSocketRoute>(this WebApplication app, string route, Func<HttpContext, Task<bool>>? routeFilter = default)
        where TWebSocketRoute : WebSocketRouteBase
    {
        app.ThrowIfNull();
        app.Map(route, async context =>
        {
            if (routeFilter is not null &&
                await routeFilter(context) is false)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            if (context.WebSockets.IsWebSocketRequest)
            {
                var logger = app.Services.GetRequiredService<ILogger<WebSocketRouteBase>>();
                var route = GetRoute<TWebSocketRoute>(context);
                try
                {
                    using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    route.WebSocket = webSocket;
                    route.Context = context;
                    await route.SocketAccepted(context.RequestAborted);
                    await HandleWebSocket(webSocket, route, context.RequestAborted);
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", context.RequestAborted);
                }
                catch(WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                {
                    logger.LogInformation("Websocket closed prematurely. Marking as closed");
                }
                catch(OperationCanceledException)
                {
                    logger.LogInformation("Websocket closed prematurely. Marking as closed");
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, "Encountered exception while handling websocket. Closing");
                }
                finally
                {
                    await route.SocketClosed();
                }
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        });

        return app;
    }

    private static async Task HandleWebSocket(WebSocket webSocket, WebSocketRouteBase route, CancellationToken cancellationToken)
    {
        var buffer = new byte[1024];
        using var memoryStream = new MemoryStream(1024);
        ValueWebSocketReceiveResult result;
        while(webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            do
            {
                var memory = new Memory<byte>(buffer);
                result = await webSocket.ReceiveAsync(memory, cancellationToken);
                await memoryStream.WriteAsync(buffer, 0, result.Count, cancellationToken);
            } while (!result.EndOfMessage);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                return;
            }

            await route.ExecuteAsync(memoryStream.ToArray(), cancellationToken);
            memoryStream.SetLength(0);
        }
    }

    private static WebSocketRouteBase GetRoute<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TWebSocketRoute>(HttpContext context)
        where TWebSocketRoute : WebSocketRouteBase
    {
        var constructors = typeof(TWebSocketRoute).GetConstructors();
        foreach(var constructor in constructors)
        {
            var dependencies = constructor.GetParameters().Select(param => context.RequestServices.GetService(param.ParameterType));
            if (dependencies.Any(d => d is null))
            {
                continue;
            }

            var route = constructor.Invoke(dependencies.ToArray());
            return route.Cast<WebSocketRouteBase>();
        }

        throw new InvalidOperationException($"Unable to resolve {typeof(TWebSocketRoute).Name}");
    }
}
