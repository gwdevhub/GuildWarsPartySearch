using GuildWarsPartySearch.Server.Attributes;
using GuildWarsPartySearch.Server.Converters;

namespace GuildWarsPartySearch.Server.Models.Endpoints;

[WebSocketConverter<JsonWebSocketMessageConverter<PartySearchList>, PartySearchList>]
public sealed class PartySearchList
{
    public List<PartySearch>? Searches { get; set; }
}
