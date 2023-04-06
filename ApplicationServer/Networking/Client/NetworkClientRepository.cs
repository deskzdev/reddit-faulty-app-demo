using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace ApplicationServer.Networking.Client;

public class NetworkClientRepository : IAsyncDisposable
{
    private readonly ILogger<NetworkClientRepository> _logger;
    private readonly ConcurrentDictionary<Guid, NetworkClient> _clients;

    public NetworkClientRepository(ILogger<NetworkClientRepository> logger)
    {
        _logger = logger;
        _clients = new ConcurrentDictionary<Guid, NetworkClient>();
    }

    public void AddClient(Guid guid, NetworkClient client)
    {
        _clients[guid] = client;
    }

    public async Task<bool> TryRemoveAsync(Guid guid)
    {
        try
        {
            var result = _clients.TryRemove(guid, out var client);
            client?.Dispose();
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            return false;
        }
    }

    public async Task DisconnectIdleClientsAsync()
    {
        var idleClients = _clients.Values
            .Where(x => x.LastPing != default && (DateTime.Now - x.LastPing).TotalSeconds >= 11)
            .ToList();

        if (idleClients.Count < 1)
        {
            return;
        }
        
        foreach (var client in idleClients)
        {
            if (!await TryRemoveAsync(client.Guid))
            {
                _logger.LogError("Failed to dispose of network client");
            }
        }
    }

    public ICollection<NetworkClient> Clients => _clients.Values;

    public async ValueTask DisposeAsync()
    {
        foreach (var client in _clients.Keys)
        {
            if (!await TryRemoveAsync(client))
            {
                _logger.LogError("Failed to dispose of network client");
            }
        }
    }
}