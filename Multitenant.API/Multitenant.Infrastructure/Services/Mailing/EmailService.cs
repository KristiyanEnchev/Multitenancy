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

        public async Task<bool> SendRegistrationEmail(EmailRequest request)
        //public async Task<bool> SendRegistrationEmail(string email, string username, string uri)
        {
            //var some = new
            //{
            //    Email = email,
            //    Username = username,
            //    Url = uri
            //};

            var content = ConverContent(request);
            var response = await httpClient.PostAsync("https://localhost:44335/api/Email/smtp-register", content);

            if (!response.IsSuccessStatusCode)
            {
            }

            return response.IsSuccessStatusCode;
        }

        private static StringContent ConverContent(object model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return content;
        }
    }
}
