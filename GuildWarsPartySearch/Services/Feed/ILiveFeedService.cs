using System.Net.WebSockets;

namespace GuildWarsPartySearch.Server.Services.Feed;

public interface ILiveFeedService
{
    Task<bool> AddClient(WebSocket webSocket, string? ipAddress, CancellationToken cancellationToken);
    void RemoveClient(WebSocket webSocket, string? ipAddress);
    Task PushUpdate(Models.PartySearch partySearchUpdate, CancellationToken cancellationToken);
}
