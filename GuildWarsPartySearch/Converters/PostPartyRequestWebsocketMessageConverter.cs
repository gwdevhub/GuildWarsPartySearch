using GuildWarsPartySearch.Server.Models.Endpoints;
using MTSC.Common.WebSockets;
using MTSC.Common.WebSockets.RoutingModules;
using Newtonsoft.Json;
using System.Text;

namespace GuildWarsPartySearch.Server.Converters;

public sealed class PostPartyRequestWebsocketMessageConverter : IWebsocketMessageConverter<PostPartySearchRequest>
{
    public PostPartyRequestWebsocketMessageConverter()
    {
    }

    public WebsocketMessage ConvertToWebsocketMessage(PostPartySearchRequest message)
    {
        var serializedData = JsonConvert.SerializeObject(message);
        var bytes = Encoding.UTF8.GetBytes(serializedData);
        return new WebsocketMessage
        {
            Data = bytes,
            Opcode = WebsocketMessage.Opcodes.Text
        };
    }

    PostPartySearchRequest IWebsocketMessageConverter<PostPartySearchRequest>.ConvertFromWebsocketMessage(WebsocketMessage websocketMessage)
    {
        var bytes = websocketMessage.Data;
        var serializedData = Encoding.UTF8.GetString(bytes);
        try
        {
            return JsonConvert.DeserializeObject<PostPartySearchRequest>(serializedData)!;
        }
        catch(Exception ex)
        {
            return default!;
        }
    }
}
