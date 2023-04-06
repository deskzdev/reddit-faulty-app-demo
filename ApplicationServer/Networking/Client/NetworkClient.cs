using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ApplicationServer.Networking.Client;

public class NetworkClient
{
    private readonly Guid _guid;
    private readonly TcpClient _tcpClient;
    private readonly NetworkClientRepository _clientRepository;
    private readonly byte[] _buffer;

    public NetworkClient(
        ILogger<NetworkClient> logger,
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
            Dispose("CLIENT_EXITED");
            return;
        }
        
        Dispose(exception.Message);
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
        var data = new byte[bytesReceived];
        Buffer.BlockCopy(_buffer, 0, data, 0, bytesReceived);

        var jsonString = Encoding.Default.GetString(data);
    }

    private async Task WriteAsync(byte[] data)
    {
        if (_disposed)
        {
            return;
        }
        
        await _tcpClient.GetStream().WriteAsync(data);
    }

    public DateTime LastPing { get; set; }
    [JsonIgnore]
    public Guid Guid => _guid;
    
    [JsonIgnore]
    public IPAddress IpAddress => IPAddress.Parse(_tcpClient.Client.RemoteEndPoint.ToString().Split(":")[0]);

    private bool _disposed;
    
    public async void Dispose(string reason)
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        await _clientRepository.TryRemoveAsync(_guid, reason);

        if (!_tcpClient.Connected)
        {
            return;
        }
        
        _tcpClient.GetStream().Close();
        _tcpClient.Close();
    }
}