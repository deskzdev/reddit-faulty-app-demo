using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Rasp1.Database;
using Serilog;

namespace ApplicationServer;

public static class Startup
{
    public static WebApplicationBuilder CreateHost()
    {
        return WebApplication.CreateBuilder();
    }

    public static async Task TestDatabaseAsync(IServiceProvider serviceProvider)
    {
        try
        {
            var databaseProvider = serviceProvider.GetRequiredService<IDatabaseProvider>();

            using var dbConnection = databaseProvider.GetConnection();
                dbConnection.SetQuery("SELECT NOW();");
            await dbConnection.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            Log.Logger.Error($"Failed database check: {e.Message}");
            Environment.Exit(-1);
        }
    }
}