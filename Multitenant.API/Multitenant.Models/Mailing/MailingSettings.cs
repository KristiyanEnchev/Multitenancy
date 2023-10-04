namespace Multitenant.Models.Mailing
{
    public class MailingSettings
    {
        public string? LinkType { get; set; }
        public string? DirectLink { get; set; }
        public string? ProxyLink { get; set; }
        public string? RegistrationEmail { get; set; }
        public string? ResetPasswordEmail { get; set; }
        public string? BaseUrl => LinkType == "direct" ? DirectLink : ProxyLink;
        public string RegistrationUrl => $"{BaseUrl}{RegistrationEmail}";
        public string ResetPasswordUrl => $"{BaseUrl}{RegistrationEmail}";

        public string? ConfirmEmailPath { get; set; }
        public string? DefaultFromEmail { get; set; }
    }
}