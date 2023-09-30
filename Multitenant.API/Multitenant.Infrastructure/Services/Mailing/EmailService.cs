namespace Multitenant.Infrastructure.Services.Mailing
{
    using System.Text;

    using Newtonsoft.Json;

    using Multitenant.Models.Mailing;
    using Multitenant.Application.Interfaces.Mailing;
    using Microsoft.Extensions.Options;

    public class EmailService : IEmailService
    {
        private readonly HttpClient httpClient;
        private readonly IOptions<MailingSettings> _mailingSettings;

        public EmailService(HttpClient httpClient, IOptions<MailingSettings> mailingSettings)
        {
            this.httpClient = httpClient;
            _mailingSettings = mailingSettings;
        }

        public async Task<HttpResponseMessage> SendRegistrationEmail(EmailRequest request)
        {
            var content = ConverContent(request);

            var response = await httpClient.PostAsync(_mailingSettings.Value.RegistrationUrl, content);

            return response;
        }

        public async Task<HttpResponseMessage> SendPasswordResetnEmail(EmailRequest request)
        {
            var content = ConverContent(request);

            var response = await httpClient.PostAsync(_mailingSettings.Value.ResetPasswordUrl, content);

            return response;
        }

        private static StringContent ConverContent(object model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return content;
        }
    }
}