namespace Multitenant.Models.Mailing
{
    public class EmailRequest
    {
        public string? From { get; set; }
        public string? To { get; set; }
        public string? TemplateName { get; set; }
        public string? ResourceUrl { get; set; }
        public IEnumerable<TemplateData>? TemplateDataList { get; set; }
    }
}
