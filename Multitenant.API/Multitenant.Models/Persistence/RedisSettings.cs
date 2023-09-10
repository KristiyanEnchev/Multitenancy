namespace Multitenant.Models.Persistence
{
    public class RedisSettings
    {
        public bool UseDistributedCache { get; set; }
        public bool PreferRedis { get; set; }
        public string? RedisURL { get; set; }

        public string KeyExpiryInMinutes { get; set; }
    }
}
