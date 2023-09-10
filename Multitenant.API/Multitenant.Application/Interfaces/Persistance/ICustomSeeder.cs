namespace Multitenant.Application.Interfaces.Persistance
{
    public interface ICustomSeeder
    {
        Task InitializeAsync(CancellationToken cancellationToken);
    }
}