using GuildWarsPartySearch.Server.Models.Endpoints;
using MTSC.Common.WebSockets;
using MTSC.Common.WebSockets.RoutingModules;
using Newtonsoft.Json;
using System.Text;

namespace GuildWarsPartySearch.Server.Converters;

public sealed class PostPartyResponseWebsocketMessageConverter : IWebsocketMessageConverter<PostPartySearchResponse>
{
    public WebsocketMessage ConvertToWebsocketMessage(PostPartySearchResponse message)
    {
        var serializedData = JsonConvert.SerializeObject(message);
        var bytes = Encoding.UTF8.GetBytes(serializedData);
        return new WebsocketMessage
        {
            Data = bytes,
            Opcode = WebsocketMessage.Opcodes.Text,
            FIN = true
        };
    }

    PostPartySearchResponse IWebsocketMessageConverter<PostPartySearchResponse>.ConvertFromWebsocketMessage(WebsocketMessage websocketMessage)
    {
        var bytes = websocketMessage.Data;
        var serializedData = Encoding.UTF8.GetString(bytes);
        return JsonConvert.DeserializeObject<PostPartySearchResponse>(serializedData)!;
    }
}
