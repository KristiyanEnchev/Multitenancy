namespace Multitenant.Models.Persistence
{
    using System.ComponentModel.DataAnnotations;

    public class DatabaseSettings : IValidatableObject
    {
        public string DBProvider { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        //public string TenantConnectionString { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(DBProvider))
            {
                yield return new ValidationResult(
                    $"{nameof(DatabaseSettings)}.{nameof(DBProvider)} is not configured",
                    new[] { nameof(DBProvider) });
            }

            if (string.IsNullOrEmpty(ConnectionString))
            {
                yield return new ValidationResult(
                    $"{nameof(DatabaseSettings)}.{nameof(ConnectionString)} is not configured",
                    new[] { nameof(ConnectionString) });
            }

            //if (string.IsNullOrEmpty(TenantConnectionString))
            //{
            //    yield return new ValidationResult(
            //        $"{nameof(DatabaseSettings)}.{nameof(TenantConnectionString)} is not configured",
            //        new[] { nameof(TenantConnectionString) });
            //}
        }
    }
}
