namespace Multitenant.Application.Identity.User
{
    public class ToggleUserStatusRequest
    {
        public bool ActivateUser { get; set; }
        public string? UserId { get; set; }
    }
}