namespace Multitenant.Infrastructure.Extensions.Swagger
{
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class SwaggerGuidSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(Guid))
            {
                schema.Type = "string";
                schema.Format = "uuid";
            }
        }
    }
}
