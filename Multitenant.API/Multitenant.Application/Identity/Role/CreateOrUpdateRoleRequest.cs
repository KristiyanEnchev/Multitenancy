namespace Multitenant.Application.Identity.Role
{
    using FluentValidation;

    using Multitenant.Application.Validations;
    using Multitenant.Application.Interfaces.Identity;

    public class CreateOrUpdateRoleRequest
    {
        public string? Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
    }

    public class CreateOrUpdateRoleRequestValidator : CustomValidator<CreateOrUpdateRoleRequest>
    {
        public CreateOrUpdateRoleRequestValidator(IRoleService roleService) =>
            RuleFor(r => r.Name)
                .NotEmpty()
                .MustAsync(async (role, name, _) => !await roleService.ExistsAsync(name, role.Id))
                    .WithMessage("Similar Role already exists.");
    }
}