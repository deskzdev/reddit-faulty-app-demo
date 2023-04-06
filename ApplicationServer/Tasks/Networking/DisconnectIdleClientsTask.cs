using ApplicationServer.Networking.Client;

namespace ApplicationServer.Tasks.Networking;

public class DisconnectIdleClientsTask : IServerTask
{
    private readonly NetworkClientRepository _clientRepository;
    public string Name => "DisconnectIdleClientsTask";
    public TimeSpan PeriodicInterval => TimeSpan.FromSeconds(10);
    public DateTime LastExecuted { get; set; }
    
    public DisconnectIdleClientsTask(NetworkClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task ExecuteAsync()
    {
        await _clientRepository
            .DisconnectIdleClientsAsync();
    }
}