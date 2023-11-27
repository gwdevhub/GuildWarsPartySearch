using GuildWarsPartySearch.Server.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Core.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Extensions;
using System.Net.WebSockets;

namespace GuildWarsPartySearch.Server.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication MapWebSocket<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TWebSocketRoute>(this WebApplication app, string route)
        where TWebSocketRoute : WebSocketRouteBase
    {
        app.ThrowIfNull();
        app.Map(route, async context =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var logger = app.Services.GetRequiredService<ILogger<WebSocketRouteBase>>();
                var route = GetRoute<TWebSocketRoute>(context);
                var routeFilters = GetRouteFilters<TWebSocketRoute>(context).ToList();

                var actionContext = new ActionContext(
                        context,
                        new RouteData(),
                        new ActionDescriptor());
                var actionExecutingContext = new ActionExecutingContext(
                        actionContext,
                        routeFilters,
                        new Dictionary<string, object?>(),
                        route);
                var actionExecutedContext = new ActionExecutedContext(
                        actionContext,
                        routeFilters,
                        route);
                try
                {
                    var processingTask = new Func<Task>(() => ProcessWebSocketRequest(route, context));
                    await BeginProcessingPipeline(actionExecutingContext, actionExecutedContext, processingTask);
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

    private static async Task ProcessWebSocketRequest(WebSocketRouteBase route, HttpContext httpContext)
    {
        using var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
        route.WebSocket = webSocket;
        route.Context = httpContext;
        await route.SocketAccepted(httpContext.RequestAborted);
        await HandleWebSocket(webSocket, route, httpContext.RequestAborted);
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", httpContext.RequestAborted);
    }

    private static async Task BeginProcessingPipeline(ActionExecutingContext actionExecutingContext, ActionExecutedContext actionExecutedContext, Func<Task> processWebSocket)
    {
        foreach (var filter in actionExecutingContext.Filters.OfType<IActionFilter>())
        {
            filter.OnActionExecuting(actionExecutingContext);
            if (actionExecutingContext.Result is IActionResult result)
            {
                await result.ExecuteResultAsync(actionExecutedContext);
                return;
            }
        }

        ActionExecutionDelegate pipeline = async () =>
        {
            await processWebSocket();
            return actionExecutedContext;
        };

        foreach (var filter in actionExecutingContext.Filters.OfType<IAsyncActionFilter>())
        {
            var next = pipeline;
            pipeline = async () =>
            {
                if (actionExecutingContext.Result is IActionResult result)
                {
                    await result.ExecuteResultAsync(actionExecutedContext);
                    return actionExecutedContext;
                }

                await filter.OnActionExecutionAsync(actionExecutingContext, next);
                return actionExecutedContext;
            };
        }

        await pipeline();
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

            await route.ExecuteAsync(result.MessageType, memoryStream.ToArray(), cancellationToken);
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

    private static IEnumerable<IFilterMetadata> GetRouteFilters<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TWebSocketRoute>(HttpContext context)
        where TWebSocketRoute : WebSocketRouteBase
    {
        foreach(var attribute in typeof(TWebSocketRoute).GetCustomAttributes(true).OfType<ServiceFilterAttribute>())
        {
            var filter = context.RequestServices.GetRequiredService(attribute.ServiceType);
            yield return filter.Cast<IFilterMetadata>();
        }
    }
}
