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
    private readonly Lazy<WebSocketMessageConverterBase> converter;

    public WebSocketRouteBase()
    {
        this.converter = new Lazy<WebSocketMessageConverterBase>(() =>
        {
            var attribute = typeof(TReceiveType).GetCustomAttributes(true).First(a => a is WebSocketConverterAttributeBase).Cast<WebSocketConverterAttributeBase>();
            var parsedConverter = GetConverter(attribute.ConverterType, this.Context!);
            return parsedConverter;
        });
    }

    public sealed override Task ExecuteAsync(WebSocketMessageType type, byte[] data, CancellationToken cancellationToken)
    {
        try
        {
            var objData = this.converter.Value.ConvertToObject(new Models.WebSocketConverterRequest { Type = type, Payload = data }).Cast<TReceiveType>();
            return this.ExecuteAsync(objData, cancellationToken);
        }
        catch(Exception ex)
        {
            var scoppedLogger = this.Context!.RequestServices.GetRequiredService<ILogger<WebSocketRouteBase<TReceiveType>>>().CreateScopedLogger(nameof(this.ExecuteAsync), string.Empty);
            scoppedLogger.LogError(ex, "Failed to process data");
            throw;
        }
    }

    public abstract Task ExecuteAsync(TReceiveType? type, CancellationToken cancellationToken);

    internal static WebSocketMessageConverterBase GetConverter(Type converterType, HttpContext context)
    {
        var constructors = converterType.GetConstructors();
        foreach (var constructor in constructors)
        {
            if (constructor.GetCustomAttributes(false).Any(a => a is DoNotInjectAttribute))
            {
                continue;
            }

            var dependencies = constructor.GetParameters().Select(param => context.RequestServices.GetService(param.ParameterType));
            if (dependencies.Any(d => d is null))
            {
                continue;
            }

            var route = constructor.Invoke(dependencies.ToArray());
            return route.Cast<WebSocketMessageConverterBase>();
        }

        throw new InvalidOperationException($"Unable to resolve {converterType.Name}");
    }
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

    public WebSocketRouteBase()
    {
        this.converter = new Lazy<WebSocketMessageConverterBase>(() =>
        {
            var attribute = typeof(TSendType).GetCustomAttributes(true).First(a => a is WebSocketConverterAttributeBase).Cast<WebSocketConverterAttributeBase>();
            var parsedConverter = GetConverter(attribute.ConverterType, this.Context!);
            return parsedConverter;
        });
    }

    public Task SendMessage(TSendType sendType, CancellationToken cancellationToken)
    {
        try
        {
            var response = this.converter.Value.ConvertFromObject(sendType!);
            return this.WebSocket!.SendAsync(response.Payload!, response.Type, response.EndOfMessage, cancellationToken);
        }
        catch(Exception ex )
        {
            var scoppedLogger = this.Context!.RequestServices.GetRequiredService<ILogger<WebSocketRouteBase<TReceiveType, TSendType>>>().CreateScopedLogger(nameof(this.SendMessage), string.Empty);
            scoppedLogger.LogError(ex, "Failed to send data");
            throw;
        }
    }
}