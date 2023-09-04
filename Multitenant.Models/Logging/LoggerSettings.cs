namespace Multitenant.Models.Logging
{
    public class LoggerSettings
    {
        public string AppName { get; set; } = "Multitenant.API";
        public string ElasticSearchUrl { get; set; } = string.Empty;
        public bool WriteToFile { get; set; } = false;
        public bool StructuredConsoleLogging { get; set; } = false;
        public string MinimumLogLevel { get; set; } = "Information";
    }
}
