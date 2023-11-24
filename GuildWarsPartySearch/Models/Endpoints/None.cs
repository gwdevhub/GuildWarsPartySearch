using GuildWarsPartySearch.Server.Converters;
using MTSC.Common.WebSockets.RoutingModules;
using System.ComponentModel;

namespace GuildWarsPartySearch.Server.Models.Endpoints;

[TypeConverter(typeof(NoneConverter))]
[WebsocketMessageConvert(typeof(NoneWebsocketMessageConverter))]
public sealed class None
{
}
