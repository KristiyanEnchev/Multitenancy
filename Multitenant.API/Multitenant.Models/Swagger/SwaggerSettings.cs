namespace Multitenant.Models.Swagger
{
    public class SwaggerSettings
    {
        public bool Enable { get; set; } = false;

        public string Version { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string TermsOfService { get; set; } = "None";

        public SwaggerContactSettings Contact { get; set; }

        public SwaggerInfrastructure Infrastructure { get; set; }

        public SwaggerSettings()
        {
            Contact = new SwaggerContactSettings();
            Infrastructure = new SwaggerInfrastructure();
        }
    }
}
