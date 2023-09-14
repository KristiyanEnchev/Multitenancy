namespace Multitenant.Host
{
    using Serilog;

    using Multitenant.Host.Startup;
    using Multitenant.Infrastructure.Extensions.Logging;
    using Multitenant.Infrastructure;
    using Multitenant.WEB;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            StaticLogger.EnsureInitialized();
            Log.Information("Server Booting Up...");
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                builder.AddConfigurations().RegisterSerilog();
                builder.Services.AddInfrastructure(builder.Configuration);
                builder.Services.AddWEB(builder.Configuration);

                var app = builder.Build();

                await app.Services.InitializeDatabasesAsync();

                app.UseInfrastructure(builder.Configuration);
                app.UseWEB(builder.Configuration);
                app.MapEndpoints();
                app.Run();
            }
            catch (Exception ex) when (!ex.GetType().Name.Equals("HostAbortedException", StringComparison.Ordinal))
            {
                StaticLogger.EnsureInitialized();
                Log.Fatal(ex, "Unhandled exception");
            }
            finally
            {
                StaticLogger.EnsureInitialized();
                Log.Information("Server Shutting down...");
                Log.CloseAndFlush();
            }
        }
    }
}