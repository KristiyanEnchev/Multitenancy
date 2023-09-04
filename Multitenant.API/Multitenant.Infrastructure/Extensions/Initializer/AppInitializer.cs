namespace Multitenant.Infrastructure.Extensions.Initializer
{
    using Microsoft.Extensions.DependencyInjection;
    using Multitenant.Application.Interfaces.Persistance;

    public static class AppInitializer
    {
        public static async Task InitializeDatabasesAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
        {
            using var scope = services.CreateScope();

            await scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>()
                .InitializeDatabasesAsync(cancellationToken);
        }
    }
}
