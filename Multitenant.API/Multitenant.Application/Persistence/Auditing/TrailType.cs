namespace Multitenant.Application.Persistence.Auditing
{
    public enum TrailType : byte
    {
        None = 0,
        Create = 1,
        Update = 2,
        Delete = 3
    }
}