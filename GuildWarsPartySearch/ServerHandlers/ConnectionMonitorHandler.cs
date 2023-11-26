using MTSC.ServerSide;
using MTSC;
using System.Net.Sockets;
using MTSC.ServerSide.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using GuildWarsPartySearch.Server.Options;

namespace GuildWarsPartySearch.Server.ServerHandlers;

public class ConnectionMonitorHandler : IHandler
{
    private bool initialized;
    private TimeSpan inactivityTimeout;

    void IHandler.ClientRemoved(MTSC.ServerSide.Server server, ClientData client) { }

    bool IHandler.HandleClient(MTSC.ServerSide.Server server, ClientData client) => false;

    bool IHandler.HandleReceivedMessage(MTSC.ServerSide.Server server, ClientData client, Message message) => false;

    bool IHandler.HandleSendMessage(MTSC.ServerSide.Server server, ClientData client, ref Message message) => false;

    bool IHandler.PreHandleReceivedMessage(MTSC.ServerSide.Server server, ClientData client, ref Message message) => false;

    void IHandler.Tick(MTSC.ServerSide.Server server)
    {
        if (!initialized)
        {
            initialized = true;
            inactivityTimeout = server.ServiceManager.GetRequiredService<IOptions<ServerOptions>>().Value.InactivityTimeout ?? TimeSpan.FromSeconds(15);
        }

        foreach (ClientData client in server.Clients)
        {
            if (DateTime.Now - client.LastActivityTime > inactivityTimeout)
            {
                server.Log("Disconnected: " + client.Socket.RemoteEndPoint?.ToString());
                client.ToBeRemoved = true;
            }
        }
    }

    private bool IsConnected(Socket client)
    {
        try
        {
            if (client is not null && client.Connected)
            {
                /* pear to the documentation on Poll:
                 * When passing SelectMode.SelectRead as a parameter to the Poll method it will return 
                 * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
                 * -or- true if data is available for reading; 
                 * -or- true if the connection has been closed, reset, or terminated; 
                 * otherwise, returns false
                 */

                // Detect if client disconnected
                if (client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] buff = new byte[1];
                    if (client.Receive(buff, SocketFlags.Peek) == 0)
                    {
                        // Client disconnected
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }
}
