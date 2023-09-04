namespace Multitenant.Models.Elmah
{
    public class ElmahSettings
    {
        public bool Activate { get; set; } = false;
        public string? ElmahDb { get; set; }
        public string? ElmahTable { get; set; }
    }
}