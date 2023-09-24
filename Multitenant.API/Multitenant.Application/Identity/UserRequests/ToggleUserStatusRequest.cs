namespace Multitenant.Application.Identity.UserRequests
{
    public class ToggleUserStatusRequest
    {
        public bool ActivateUser { get; set; }
        public string? UserId { get; set; }
    }
}