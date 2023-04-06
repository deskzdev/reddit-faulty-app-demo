using ApplicationServer.Networking;
using ApplicationServer.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ApplicationServer;

internal static class Program
{
    private static async Task Main()
    {
        var builder = WebApplication.CreateBuilder();
        
        builder.Services.AddControllers();
        
        ServiceCollection.AddServices(builder.Services, builder.Configuration);
        
        var app = builder.Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();
        
        Log.Logger.Information("Application is booting up...");
        
        var services = app.Services;
        
        services.GetRequiredService<ServerTaskWorker>().Start();

        var networkServer = services.GetRequiredService<NetworkListener>();
        networkServer.Start();
        
        networkServer.ListenAsync();
        
        Log.Logger.Information("Listening for connections!");
        
        await app.RunAsync();
    }
}