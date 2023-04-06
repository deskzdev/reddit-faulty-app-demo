using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationServer.Networking.Client;

public class NetworkClientFactory
{
    private readonly IServiceProvider _serviceProvider;

    public NetworkClientFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public NetworkClient CreateClient(Guid guid, TcpClient tcpClient)
    {
        return ActivatorUtilities.CreateInstance<NetworkClient>(_serviceProvider, guid, tcpClient);
    }
}