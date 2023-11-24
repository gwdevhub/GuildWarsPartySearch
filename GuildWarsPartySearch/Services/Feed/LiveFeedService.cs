using GuildWarsPartySearch.Server.Models.Endpoints;
using MTSC.Common.WebSockets;
using MTSC.ServerSide;
using Newtonsoft.Json;
using System.Text;

namespace GuildWarsPartySearch.Server.Services.Feed;

public sealed class LiveFeedService : ILiveFeedService
{
    private static readonly List<ClientData> Clients = [];

    public void AddClient(ClientData client)
    {
        AddClientInternal(client);
    }

    public void PushUpdate(MTSC.ServerSide.Server server, PartySearchUpdate partySearchUpdate)
    {
        var payloadString = JsonConvert.SerializeObject(partySearchUpdate);
        var payload = Encoding.UTF8.GetBytes(payloadString);
        var websocketMessage = new WebsocketMessage
        {
            Data = payload,
            FIN = true,
            Opcode = WebsocketMessage.Opcodes.Text
        };
        var messageBytes = websocketMessage.GetMessageBytes();
        ExecuteOnClientsInternal(client =>
        {
            server.QueueMessage(client, messageBytes);
        });
    }

    public void RemoveClient(ClientData client)
    {
        RemoveClientInternal(client);
    }

    private static void AddClientInternal(ClientData clientData)
    {
        lock (Clients)
        {
            Clients.Add(clientData);
        }
    }

    private static void RemoveClientInternal(ClientData clientData)
    {
        lock (Clients)
        {
            Clients.Remove(clientData);
        }
    }

    private static void ExecuteOnClientsInternal(Action<ClientData> action)
    {
        lock (Clients)
        {
            foreach (var client in Clients)
            {
                action(client);
            }
        }
    }
}
