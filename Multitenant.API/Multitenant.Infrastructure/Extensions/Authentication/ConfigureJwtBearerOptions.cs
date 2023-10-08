namespace Multitenant.Infrastructure.Extensions.Authentication
{
    using System.Net;
    using System.Text;
    using System.Security.Claims;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.AspNetCore.Authentication.JwtBearer;

    using Multitenant.Application.Exceptions;
    using Multitenant.Models.Security;
    using Serilog;

    public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly JwtSettings _jwtSettings;

        public ConfigureJwtBearerOptions(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public void Configure(JwtBearerOptions options)
        {
            Configure(string.Empty, options);
        }

        public void Configure(string? name, JwtBearerOptions options)
        {
            if (name != JwtBearerDefaults.AuthenticationScheme)
            {
                return;
            }

            //byte[] key = Encoding.ASCII.GetBytes(_jwtSettings.Key);
            byte[] key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateLifetime = true,
                ValidateAudience = true,
                RoleClaimType = ClaimTypes.Role,
                ClockSkew = TimeSpan.Zero,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience
            };
            options.Events = new JwtBearerEvents()
            {
                //                OnAuthenticationFailed = c =>
                //                {
                //                    if (c.Request.Path.Value == "/api/auth/logout" && !c.Response.HasStarted)
                //                    {
                //                        return Task.CompletedTask;
                //                    }
                //                    if (c.Exception is SecurityTokenExpiredException)
                //                    {
                //                        c.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                //                        c.Response.ContentType = "application/json";
                //                        throw new UnauthorizedException("Authentication Failed.");
                //                    }
                //                    else
                //                    {
                //#if DEBUG
                //                        c.NoResult();
                //                        c.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                //                        c.Response.ContentType = "text/plain";
                //                        throw new InternalServerException("Authentication Failed.");
                //                        //return c.Response.WriteAsync(c.Exception.ToString());
                //#else
                //                                c.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                //                                c.Response.ContentType = "application/json";
                //                                throw new InternalServerException("Authentication Failed.");
                //#endif
                //                    }
                //                },
                OnAuthenticationFailed = c =>
                {
                    if (c.Request.Path.Value == "/api/auth/logout" && !c.Response.HasStarted)
                    {
                        return Task.CompletedTask;
                    }
                    if (c.Exception is SecurityTokenExpiredException)
                    {
                        c.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        c.Response.ContentType = "application/json";
                        // Log the exception details
                        Log.Error(c.Exception, "Authentication Failed: Token has expired.");
                    }
                    else
                    {
#if DEBUG
                        c.NoResult();
                        c.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        c.Response.ContentType = "text/plain";
                        // Log the exception details
                        Log.Error(c.Exception, "Authentication Failed: Internal Server Error.");
#else
                        c.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        c.Response.ContentType = "application/json";
                        // Log the exception details
                        Log.Error(c.Exception, "Authentication Failed: Internal Server Error.");
#endif
                    }
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        context.Response.ContentType = "application/json";
                        throw new UnauthorizedException("Authentication Failed.");
                    }

                    return Task.CompletedTask;
                },
                OnForbidden = context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    context.Response.ContentType = "application/json";
                    throw new ForbiddenException("You are not authorized to access this resource.");
                },
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];

                    if (!string.IsNullOrEmpty(accessToken) &&
                        context.HttpContext.Request.Path.StartsWithSegments("/notifications"))
                    {
                        // Read the token out of the query string
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        }
    }
}