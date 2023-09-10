namespace Multitenant.Application.Interfaces.Persistance
{
    public interface IConnectionStringSecurer
    {
        string? MakeSecure(string? connectionString, string? dbProvider = null);
    }
}
