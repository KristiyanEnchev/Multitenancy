namespace Multitenant.WEB.Extensions.Swagger
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OpenApi.Models;

    using Swashbuckle.AspNetCore.SwaggerUI;

    using Multitenant.Models.Swagger;
    using Multitenant.WEB.Filters.Swagger;

    public static class SwaggerConfigExtension
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services, IConfiguration configuration)
        {
            SwaggerSettings settings = configuration.GetSection(nameof(SwaggerSettings)).Get<SwaggerSettings>()!;

            services.AddEndpointsApiExplorer();

            if (settings == null)
            {
                return services;
            }

            if (settings.Enable)
            {
                services.AddSwaggerGen(c =>
                {
                    c.EnableAnnotations();

                    // USE FOR Documentation ---- need to create the file
                    //var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    //c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

                    //c.SwaggerDoc(settings.Version, new OpenApiInfo
                    //{
                    //    Version = settings.Version,
                    //    Title = settings.Title,
                    //    Description = settings.Description,
                    //    Contact = new OpenApiContact
                    //    {
                    //        Name = settings.Contact?.Name ?? "",
                    //        Email = settings.Contact?.Email ?? "",
                    //        Url = settings.Contact?.Url != null ? new Uri(settings.Contact.Url) : null
                    //    }
                    //});

                    if (configuration["SecuritySettings:Provider"]!.Equals("AzureAd", StringComparison.OrdinalIgnoreCase))
                    {
                        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                        {
                            Type = SecuritySchemeType.OAuth2,
                            Flows = new OpenApiOAuthFlows
                            {
                                AuthorizationCode = new OpenApiOAuthFlow
                                {
                                    AuthorizationUrl = new Uri(configuration["SecuritySettings:Swagger:AuthorizationUrl"]!),
                                    TokenUrl = new Uri(configuration["SecuritySettings:Swagger:TokenUrl"]!),
                                    Scopes = new Dictionary<string, string>
                                    {
                                        { configuration["SecuritySettings:Swagger:ApiScope"]!, "access the api" }
                                    }
                                }
                            },
                            Description = "OAuth2.0 Auth Code with PKCE"
                        });

                        // Define the requirement for OAuth2 authentication
                        c.AddSecurityRequirement(new OpenApiSecurityRequirement
                        {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "Bearer"
                                    }
                                },
                                new string[] { }
                            }
                        });
                    }
                    else
                    {
                        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                        {
                            Name = "Authorization",
                            In = ParameterLocation.Header,
                            Type = SecuritySchemeType.Http,
                            Scheme = "Bearer",
                            BearerFormat = "JWT",
                            Description = "Input your Bearer token in this format - Bearer {your token here} to access this API",
                        });
                        c.AddSecurityRequirement(new OpenApiSecurityRequirement
                        {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "Bearer",
                                    },
                                    Scheme = "Bearer",
                                    Name = "Bearer",
                                    In = ParameterLocation.Header,
                                }, new List<string>()
                            },
                        });
                    }

                    // Add the custom operation filter
                    c.OperationFilter<SwaggerHeaderAttributeFilter>();
                    c.OperationFilter<SwaggerGlobalAuthFilter>("Bearer");
                    c.SchemaFilter<SwaggerGuidSchemaFilter>();
                });
            }

            return services;
        }

        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app, IConfiguration config)
        {
            if (config.GetValue<bool>("SwaggerSettings:Enable"))
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Multitenant.API");

                    // Customize Swagger UI options
                    options.DefaultModelsExpandDepth(-1);
                    options.DocExpansion(DocExpansion.None);

                    if (config["SecuritySettings:Provider"]!.Equals("AzureAd", StringComparison.OrdinalIgnoreCase))
                    {
                        options.OAuthClientId(config["SecuritySettings:Swagger:OpenIdClientId"]);
                        options.OAuthUsePkce();
                        options.OAuthScopes(config["SecuritySettings:Swagger:ApiScope"]);
                    }
                });

                //app.Use(async (context, next) =>
                //{
                //    if (context.Request.Path == "/")
                //    {
                //        context.Response.Redirect("/swagger/index.html");
                //    }

                //    await next();
                //});
            }

            return app;
        }
    }
}
