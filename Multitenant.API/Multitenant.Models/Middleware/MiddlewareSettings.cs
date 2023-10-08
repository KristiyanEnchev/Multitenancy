namespace Multitenant.Models.Middleware
{
    public class MiddlewareSettings
    {
        public bool EnableHttpsLogging { get; set; } = false;
        public bool EnableBackgroundExpiryJobRunner { get; set; } = false;
    }
}