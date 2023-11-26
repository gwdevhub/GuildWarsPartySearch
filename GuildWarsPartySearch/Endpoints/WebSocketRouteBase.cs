using GuildWarsPartySearch.Server.Attributes;
using GuildWarsPartySearch.Server.Converters;
using System.Extensions;
using System.Net.WebSockets;

namespace GuildWarsPartySearch.Server.Endpoints;

public abstract class WebSocketRouteBase
{
    public HttpContext? Context { get; internal set; }

    public WebSocket? WebSocket { get; internal set; }

    public virtual Task SocketAccepted(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public virtual Task SocketClosed()
    {
        return Task.CompletedTask;
    }

    public abstract Task ExecuteAsync(WebSocketMessageType type, byte[] data, CancellationToken cancellationToken);
}

public abstract class WebSocketRouteBase<TReceiveType> : WebSocketRouteBase
    where TReceiveType : class, new()
{
    private readonly Lazy<WebSocketMessageConverterBase> converter = new(() =>
    {
        var attribute = typeof(TReceiveType).GetCustomAttributes(true).First(a => a is WebSocketConverterAttributeBase).Cast<WebSocketConverterAttributeBase>();
        var converter = Activator.CreateInstance(attribute.ConverterType)!.Cast<WebSocketMessageConverterBase>();
        return converter;
    });

    public sealed override Task ExecuteAsync(WebSocketMessageType type, byte[] data, CancellationToken cancellationToken)
    {
        try
        {
            var objData = this.converter.Value.ConvertToObject(new Models.WebSocketConverterRequest { Type = type, Payload = data }).Cast<TReceiveType>();
            return this.ExecuteAsync(objData, cancellationToken);
        }
        catch(Exception ex)
        {
            var scoppedLogger = this.Context!.RequestServices.GetRequiredService<ILogger<WebSocketRouteBase>>().CreateScopedLogger(nameof(this.ExecuteAsync), string.Empty);
            scoppedLogger.LogError(ex, "Failed to process data");
            throw;
        }
    }

    public abstract Task ExecuteAsync(TReceiveType? type, CancellationToken cancellationToken);
}

public abstract class WebSocketRouteBase<TReceiveType, TSendType> : WebSocketRouteBase<TReceiveType>
    where TReceiveType : class, new()
{
    private readonly Lazy<WebSocketMessageConverterBase> converter = new(() =>
    {
        var attribute = typeof(TSendType).GetCustomAttributes(true).First(a => a is WebSocketConverterAttributeBase).Cast<WebSocketConverterAttributeBase>();
        var converter = Activator.CreateInstance(attribute.ConverterType)!.Cast<WebSocketMessageConverterBase>();
        return converter;
    });

    public Task SendMessage(TSendType sendType, CancellationToken cancellationToken)
    {
        try
        {
            var response = this.converter.Value.ConvertFromObject(sendType!);
            return this.WebSocket!.SendAsync(response.Payload!, response.Type, response.EndOfMessage, cancellationToken);
        }
        catch(Exception ex )
        {
            var scoppedLogger = this.Context!.RequestServices.GetRequiredService<ILogger<WebSocketRouteBase>>().CreateScopedLogger(nameof(this.SendMessage), typeof(TSendType).Name);
            scoppedLogger.LogError(ex, "Failed to send data");
            throw;
        }
    }
}