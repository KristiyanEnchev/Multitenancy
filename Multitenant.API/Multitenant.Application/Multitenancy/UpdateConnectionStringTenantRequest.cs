namespace Multitenant.Application.Multitenancy
{
    using FluentValidation;

    using MediatR;

    using Multitenant.Application.Validations;
    using Multitenant.Application.Interfaces.Tenant;
    using Multitenant.Application.Interfaces.Persistance;

    public class UpdateConnectionStringTenantRequest : IRequest<string>
    {
        public string TenantId { get; set; } = default!;
        public string? ConnectionString { get; set; }
    }

    public class UpdateConnectionStringTenantRequestValidator : CustomValidator<UpdateConnectionStringTenantRequest>
    {
        public UpdateConnectionStringTenantRequestValidator(
            IConnectionStringValidator connectionStringValidator)
        {
            RuleFor(t => t.TenantId)
                .NotEmpty();

            RuleFor(t => t.ConnectionString).Cascade(CascadeMode.Stop)
                .Must((_, cs) => string.IsNullOrWhiteSpace(cs) || connectionStringValidator.TryValidate(cs))
                .WithMessage("Connection string invalid.");
        }
    }

    public class UpdateConnectionStringTenantRequestHandler : IRequestHandler<UpdateConnectionStringTenantRequest, string>
    {
        private readonly ITenantService _tenantService;

        public UpdateConnectionStringTenantRequestHandler(ITenantService tenantService) => _tenantService = tenantService;


        public Task<string> Handle(UpdateConnectionStringTenantRequest request, CancellationToken cancellationToken) =>
            _tenantService.UpdateConnectionString(request, cancellationToken);
    }
}