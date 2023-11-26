using Newtonsoft.Json;
using System.Extensions;
using System.Net.WebSockets;
using System.Text;

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

    public abstract Task ExecuteAsync(byte[] data, CancellationToken cancellationToken);
}

public abstract class WebSocketRouteBase<TReceiveType> : WebSocketRouteBase
    where TReceiveType : class, new()
{
    public sealed override Task ExecuteAsync(byte[] data, CancellationToken cancellationToken)
    {
        var stringData = Encoding.UTF8.GetString(data);
        try
        {
            var objData = JsonConvert.DeserializeObject<TReceiveType>(stringData);
            return this.ExecuteAsync(objData, cancellationToken);
        }
        catch(Exception ex)
        {
            var scoppedLogger = this.Context!.RequestServices.GetRequiredService<ILogger<WebSocketRouteBase>>().CreateScopedLogger(nameof(this.ExecuteAsync), string.Empty);
            scoppedLogger.LogError(ex, "Failed to process data");
            scoppedLogger.LogDebug($"Failed to process data. Content: {stringData}");
            throw;
        }
    }

    public abstract Task ExecuteAsync(TReceiveType? type, CancellationToken cancellationToken);
}

public abstract class WebSocketRouteBase<TReceiveType, TSendType> : WebSocketRouteBase<TReceiveType>
    where TReceiveType : class, new()
{
    public Task SendMessage(TSendType sendType, CancellationToken cancellationToken)
    {
        try
        {
            var serialized = JsonConvert.SerializeObject(sendType);
            var data = Encoding.UTF8.GetBytes(serialized);
            return this.WebSocket!.SendAsync(data, WebSocketMessageType.Text, true, cancellationToken);
        }
        catch(Exception ex )
        {
            var scoppedLogger = this.Context!.RequestServices.GetRequiredService<ILogger<WebSocketRouteBase>>().CreateScopedLogger(nameof(this.SendMessage), typeof(TSendType).Name);
            scoppedLogger.LogError(ex, "Failed to send data");
            throw;
        }
    }
}