namespace Multitenant.WEB.Filters.Swagger
{
    using System.Reflection;

    using Microsoft.OpenApi.Any;
    using Microsoft.OpenApi.Models;

    using Swashbuckle.AspNetCore.SwaggerGen;

    using Multitenant.WEB.Attributes;

    public class SwaggerHeaderAttributeFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attributes = context.MethodInfo.GetCustomAttributes<SwaggerHeaderAttribute>();

            foreach (var attribute in attributes)
            {
                var existingParam = operation.Parameters.FirstOrDefault(p =>
                    p.In == ParameterLocation.Header && p.Name == attribute.HeaderName);

                if (existingParam != null)
                {
                    operation.Parameters.Remove(existingParam);
                }

                var headerParam = new OpenApiParameter
                {
                    Name = attribute.HeaderName,
                    In = ParameterLocation.Header,
                    Description = attribute.Description,
                    Required = attribute.IsRequired,
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Default = new OpenApiString(attribute.DefaultValue)
                    }
                };

                operation.Parameters.Add(headerParam);
            }
        }
    }
}
