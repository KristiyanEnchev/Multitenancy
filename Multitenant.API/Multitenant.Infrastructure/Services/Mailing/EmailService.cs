using Multitenant.Application.Interfaces.Mailing;
using Multitenant.Application.Persistence.Auditing;
using Multitenant.Domain.Entities.Identity;
using Multitenant.Models.Mailing;
using Newtonsoft.Json;
using System.Security.Policy;
using System.Text;

namespace Multitenant.Infrastructure.Services.Mailing
{
    public class EmailService : IEmailService
    {
        private readonly HttpClient httpClient;
        public EmailService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> SendRegistrationEmail(EmailRequest request)
        {
            var content = ConverContent(request);

            var response = await httpClient.PostAsync("https://localhost/mailing/api/Email/smtp-register", content);

            return response;
        }

        public async Task<HttpResponseMessage> SendPasswordResetnEmail(EmailRequest request)
        {
            var content = ConverContent(request);

            var response = await httpClient.PostAsync("https://localhost/mailing/api/Email/smtp-resset-password", content);

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
