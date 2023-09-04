namespace Multitenant.Infrastructure.Extensions.Swagger
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.OpenApi.Models;

    using Swashbuckle.AspNetCore.SwaggerGen;

    public class SwaggerGlobalAuthFilter : IOperationFilter
    {
        private readonly string _authenticationScheme;

        public SwaggerGlobalAuthFilter(string authenticationScheme)
        {
            _authenticationScheme = authenticationScheme;
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check if the AllowAnonymous attribute is present on the action method
            var hasAllowAnonymousAttribute = context.MethodInfo.GetCustomAttributes(true)
                .OfType<AllowAnonymousAttribute>()
                .Any();

            if (!hasAllowAnonymousAttribute)
            {
                // Add the authentication requirement to the operation if not already present
                if (operation.Security == null)
                {
                    operation.Security = new List<OpenApiSecurityRequirement>();
                }

                var securityRequirement = new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = _authenticationScheme
                        }
                    },
                    new List<string>()
                }
            };

                operation.Security.Add(securityRequirement);
            }
        }
    }
}
