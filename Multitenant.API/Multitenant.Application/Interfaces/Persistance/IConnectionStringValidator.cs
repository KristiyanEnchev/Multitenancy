namespace Multitenant.Application.Interfaces.Persistance
{
    public interface IConnectionStringValidator
    {
        bool TryValidate(string connectionString, string? dbProvider = null);
    }
}
