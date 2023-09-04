namespace Multitenant.Application.Interfaces.Utility.Serializer
{
    using Multitenant.Application.Interfaces.DependencyScope;

    public interface ISerializerService : ITransientService
    {
        string Serialize<T>(T obj);

        string Serialize<T>(T obj, Type type);

        T Deserialize<T>(string text);
    }
}