using System.Net.Sockets;

namespace ApplicationServer.Networking.Client;

public class NetworkClient
{
    private readonly Guid _guid;
    private readonly TcpClient _tcpClient;
    private readonly byte[] _buffer;
    private Task _processingLoop;

    public NetworkClient(
        Guid guid, 
        TcpClient tcpClient)
    {
        _guid = guid;
        _tcpClient = tcpClient;
        _buffer = new byte[4096];

    }

    private async Task StartListening(Func<Exception, Task> onExceptionAsync, Func<int, Task> onReceivedAsync, byte[] buffer)
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
            await onExceptionAsync(e);
        }
    }

    private async Task OnExceptionAsync(Exception exception)
    {
        if (exception.GetBaseException().GetType() == typeof(SocketException) && ((SocketException)exception).ErrorCode == 10054)
        {
            await DisposeAsync();
            return;
        }
        
        Console.WriteLine(exception);
        await DisposeAsync();
    }

    public Task ListenAsync()
    {
        _processingLoop = Task.Run(() => StartListening(OnExceptionAsync, OnReceivedAsync, _buffer));
        return Task.CompletedTask;
    }
    
    private async Task OnReceivedAsync(int bytesReceived)
    {
        // Removed implementation for simplicity
    }

    public DateTime LastPing { get; set; }
    public Guid Guid => _guid;
    
    private bool _disposed;
    
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (!_tcpClient.Connected)
        {
            return;
        }
        
        await _tcpClient.GetStream().DisposeAsync();
        _tcpClient.Close();
    }
}