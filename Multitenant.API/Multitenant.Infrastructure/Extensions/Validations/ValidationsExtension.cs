namespace Multitenant.Infrastructure.Extensions.Validations
{
    using System.Reflection;

    using Microsoft.Extensions.DependencyInjection;

    using MediatR;

    using Multitenant.Application.Validations;

    public static class ValidationsExtension
    {
        public static IServiceCollection AddBehaviours(this IServiceCollection services, Assembly assemblyContainingValidators)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            return services;
        }
    }
}