using System.Net.WebSockets;

namespace GuildWarsPartySearch.Server.Services.Feed;

public interface ILiveFeedService
{
    void AddClient(WebSocket webSocket);
    void RemoveClient(WebSocket webSocket);
    Task PushUpdate(Models.PartySearch partySearchUpdate, CancellationToken cancellationToken);
}
