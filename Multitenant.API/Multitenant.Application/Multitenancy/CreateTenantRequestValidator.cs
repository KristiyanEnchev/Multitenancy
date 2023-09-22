namespace Multitenant.Application.Multitenancy
{
    using FluentValidation;

    using Multitenant.Application.Interfaces.Persistance;
    using Multitenant.Application.Interfaces.Tenant;
    using Multitenant.Application.Validations;

    public class CreateTenantRequestValidator : CustomValidator<CreateTenantRequest>
    {
        public CreateTenantRequestValidator(
            ITenantService tenantService,
            IConnectionStringValidator connectionStringValidator)
        {
            RuleFor(t => t.Id).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MustAsync(async (id, _) => !await tenantService.ExistsWithIdAsync(id))
                    .WithMessage((_, id) => $"Tenant {id} already exists.");

            RuleFor(t => t.Name).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MustAsync(async (name, _) => !await tenantService.ExistsWithNameAsync(name!))
                    .WithMessage((_, name) => $"Tenant {name} already exists.");

            RuleFor(t => t.ConnectionString).Cascade(CascadeMode.Stop)
                .Must((_, cs) => string.IsNullOrWhiteSpace(cs) || connectionStringValidator.TryValidate(cs))
                    .WithMessage("Connection string invalid.");

            RuleFor(t => t.AdminEmail).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .EmailAddress();
        }
    }
}