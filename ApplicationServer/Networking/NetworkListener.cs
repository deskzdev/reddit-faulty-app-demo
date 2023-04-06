using System.Net.Sockets;
using ApplicationServer.Networking.Client;
using Microsoft.Extensions.Logging;

namespace ApplicationServer.Networking;

public class NetworkListener
{
    private readonly ILogger<NetworkListener> _logger;
    private readonly TcpListener _listener;
    private readonly NetworkClientRepository _clientRepository;

    public NetworkListener(
        ILogger<NetworkListener> logger, 
        TcpListener listener, 
        NetworkClientRepository clientRepository)
    {
        _logger = logger;
        _listener = listener;
        _clientRepository = clientRepository;
    }

    public void Start(int backlog = 100)
    {
        _listener.Start(backlog);
    }

    public async Task ListenAsync()
    {
        _logger.LogInformation("Listening for connections!");
        
        while (true)
        {
            var client = await _listener.AcceptTcpClientAsync();
            await AcceptClient(client);
        }
    }
        
    private async Task AcceptClient(TcpClient client)
    {
        var clientId = Guid.NewGuid();
        var networkClient = new NetworkClient(clientId, client, _clientRepository);
        
        _clientRepository.AddClient(clientId, networkClient);

        _logger.LogWarning("Someone is trying to connect...");
        
        await networkClient.ListenAsync();
    }
}