using GuildWarsPartySearch.Server.Models.Endpoints;
using MTSC.ServerSide;

namespace GuildWarsPartySearch.Server.Services.Feed;

public interface ILiveFeedService
{
    void AddClient(ClientData client);
    void RemoveClient(ClientData client);
    void PushUpdate(MTSC.ServerSide.Server server, PartySearchUpdate partySearchUpdate);
}
