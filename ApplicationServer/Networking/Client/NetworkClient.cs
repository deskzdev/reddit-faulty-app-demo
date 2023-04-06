using System.Net.Sockets;

namespace ApplicationServer.Networking.Client;

public class NetworkClient
{
    private readonly Guid _guid;
    private readonly TcpClient _tcpClient;
    private readonly NetworkClientRepository _clientRepository;
    private readonly byte[] _buffer;

    public NetworkClient(
        Guid guid, 
        TcpClient tcpClient,
        NetworkClientRepository clientRepository)
    {
        _guid = guid;
        _tcpClient = tcpClient;
        _clientRepository = clientRepository;
        _buffer = new byte[4096];
    }

    private async void StartListening(Action<Exception> onException, Func<int, Task> onReceivedAsync, byte[] buffer)
    {
        try
        {
            while (_tcpClient.Connected)
            {
                var bytes = await _tcpClient.Client.ReceiveAsync(buffer, SocketFlags.None);
                
                if (bytes > 0)
                {
                    await onReceivedAsync(bytes);
                }

                await Task.Delay(10);
            }
        }
        catch (Exception e)
        {
            onException(e);
        }
    }

    private void OnException(Exception exception)
    {
        if (exception.GetBaseException().GetType() == typeof(SocketException) && ((SocketException)exception).ErrorCode == 10054)
        {
            Dispose();
            return;
        }
        
        Console.WriteLine(exception);
        Dispose();
    }

    public Task ListenAsync()
    {
        var onException = new Action<Exception>(OnException);
        var onReceived = new Func<int, Task>(OnReceivedAsync);
        
        var listeningThread = new Thread(() => StartListening(onException, onReceived, _buffer))
        {
            Name = "Client Listening Thread",
            Priority = ThreadPriority.AboveNormal
        };
        
        listeningThread.Start();
        return Task.CompletedTask;
    }
    
    private async Task OnReceivedAsync(int bytesReceived)
    {
        // Removed implementation for simplicity
    }

    public DateTime LastPing { get; set; }
    public Guid Guid => _guid;
    
    private bool _disposed;
    
    public async void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        await _clientRepository.TryRemoveAsync(_guid);

        if (!_tcpClient.Connected)
        {
            return;
        }
        
        _tcpClient.GetStream().Close();
        _tcpClient.Close();
    }
}