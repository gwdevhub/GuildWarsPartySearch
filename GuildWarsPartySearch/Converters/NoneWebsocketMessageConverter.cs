using GuildWarsPartySearch.Server.Models.Endpoints;
using MTSC.Common.WebSockets;
using MTSC.Common.WebSockets.RoutingModules;

namespace GuildWarsPartySearch.Server.Converters;

public sealed class NoneWebsocketMessageConverter : IWebsocketMessageConverter<None>
{
    public None ConvertFromWebsocketMessage(WebsocketMessage websocketMessage)
    {
        return new None();
    }

    public WebsocketMessage ConvertToWebsocketMessage(None message)
    {
        return new WebsocketMessage { FIN = true };
    }
}
