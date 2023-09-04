using Microsoft.Extensions.DependencyInjection;
using Multitenant.Application.Interfaces.Utility.User;
using Multitenant.Infrastructure.Services.Utility;

namespace Multitenant.Infrastructure
{
    public static class Startup
    {
        //services.AddEndpointsApiExplorer();

        private static IServiceCollection AddCurrentUser(this IServiceCollection services) =>
            services
            .AddScoped<ICurrentUser, CurrentUser>()
            .AddScoped(sp => (ICurrentUserInitializer)sp.GetRequiredService<ICurrentUser>());

    }
}