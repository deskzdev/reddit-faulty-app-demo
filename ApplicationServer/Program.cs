using System.Net;
using System.Net.Sockets;
using ApplicationServer.Networking;
using ApplicationServer.Networking.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ApplicationServer;

internal static class Program
{
    private static async Task Main()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        var builder = Host
            .CreateDefaultBuilder()
            .ConfigureServices((context, serviceCollection) =>
            {
                serviceCollection.AddSingleton(new TcpListener(IPAddress.Parse("0.0.0.0"), 1234));
                serviceCollection.AddTransient<NetworkClient>();
                serviceCollection.AddSingleton<NetworkClientFactory>();
                serviceCollection.AddSingleton<NetworkClientRepository>();
                serviceCollection.AddSingleton<NetworkListener>();
            }).Build();
        
        Log.Logger.Information("Application is booting up...");

        var services = builder.Services;
        
        var networkServer = services.GetRequiredService<NetworkListener>();
        networkServer.Start();
        
        await networkServer.ListenAsync();
    }
}