namespace Multitenant.Infrastructure.Services.Identity.Authentication.Util
{
    using System.Text;

    using Microsoft.Extensions.Options;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.WebUtilities;

    using Multitenant.Shared;
    using Multitenant.Models.Mailing;
    using Multitenant.Shared.Persistance;
    using Multitenant.Domain.Entities.Identity;
    using Multitenant.Application.Exceptions;
    using Multitenant.Shared.Constants.Multitenancy;

    internal class Util : IUtil
    {
        private readonly UserManager<User> _userManager;
        private readonly MultiTenantInfo? _currentTenant;
        private readonly IOptions<MailingSettings> _mailingSettings;

        public Util(IOptions<MailingSettings> mailingSettings, MultiTenantInfo? currentTenant, UserManager<User> userManager)
        {
            _mailingSettings = mailingSettings;
            _currentTenant = currentTenant;
            _userManager = userManager;
        }

        public async Task<string> GetEmailVerificationUriAsync(User user, string origin)
        {
            EnsureValidTenant();

            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            string route = _mailingSettings.Value.ConfirmEmailPath!;

            var endpointUri = new Uri(string.Concat($"{origin}/", route));

            string verificationUri = QueryHelpers.AddQueryString(endpointUri.ToString(), QueryStringKeys.UserId, user.Id);

            verificationUri = QueryHelpers.AddQueryString(verificationUri, QueryStringKeys.Code, code);

            verificationUri = QueryHelpers.AddQueryString(verificationUri, MultitenancyConstants.TenantIdName, _currentTenant?.Id!);

            return verificationUri;
        }

        public void EnsureValidTenant()
        {
            if (string.IsNullOrWhiteSpace(_currentTenant?.Id))
            {
                throw new UnauthorizedException("Invalid Tenant.");
            }
        }

        public EmailRequest CreateRegistrationEmailRequest(User user, string emailVerificationUri)
        {
            var request = new EmailRequest
            {
                From = _mailingSettings.Value.DefaultFromEmail!,
                To = user.Email,
                TemplateName = TemplateNames.email_confirmation.ToString().Replace("_", "-"),
                TemplateDataList = new List<TemplateData>
                {
                    new TemplateData
                    {
                        Key = "UserName",
                        Value = user.UserName
                    },
                    new TemplateData
                    {
                        Key = "Email",
                        Value = user.Email
                    },
                     new TemplateData
                    {
                        Key = "Url",
                        Value = emailVerificationUri
                    }
                }
            };

            return request;
        }

        public EmailRequest CreateRessetPasswordEmailRequest(User user, string code, string endpointUri, string passwordResetUrl)
        {
            var request = new EmailRequest
            {
                From = _mailingSettings.Value.DefaultFromEmail!,
                To = user.Email,
                TemplateName = TemplateNames.passwordReset.ToString(),
                TemplateDataList = new List<TemplateData>
                {
                    new TemplateData
                    {
                        Key = "UserName",
                        Value = user.UserName
                    },
                    new TemplateData
                    {
                        Key = "Email",
                        Value = user.Email
                    },
                    new TemplateData
                    {
                        Key = "Code",
                        Value = code
                    },
                    new TemplateData
                    {
                        Key = "EndpointUri",
                        Value = endpointUri
                    },
                    new TemplateData
                    {
                        Key = "PasswordResetUrl",
                        Value = passwordResetUrl
                    }
                }
            };

            return request;
        }
    }
}
