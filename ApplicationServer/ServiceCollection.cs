using System.Net;
using System.Net.Sockets;
using ApplicationServer.Networking;
using ApplicationServer.Networking.Client;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace ApplicationServer;

public static class ServiceCollection
{
    public static void AddServices(IServiceCollection serviceCollection, IConfiguration config)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .CreateLogger();
        
        var host = config["Networking:Host"] ?? IPAddress.Any.ToString();
        var port = int.Parse(config["Networking:Port"]);
        
        serviceCollection.AddSingleton(new TcpListener(
            IPAddress.Parse(host), port
        ));
        
        serviceCollection.AddTransient<NetworkClient>();
        serviceCollection.AddSingleton<NetworkClientFactory>();
        serviceCollection.AddSingleton<NetworkClientRepository>();
        serviceCollection.AddSingleton<NetworkListener>();
    }
}