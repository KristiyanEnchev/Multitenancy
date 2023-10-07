namespace Multitenant.Application.Identity.Role
{
    using FluentValidation;

    using Multitenant.Application.Interfaces.Identity;
    using Multitenant.Application.Validations;

    public class AddPermissionToRoleRequest
    {
        public string RoleId { get; set; } = default!;
        public int PermissionId { get; set; }
    }

    public class AddPermissionToRoleRequestValidator : CustomValidator<AddPermissionToRoleRequest>
    {
        public AddPermissionToRoleRequestValidator(IPermissionService permissionService)
        {
            RuleFor(r => r.RoleId)
                .NotEmpty();
            RuleFor(r => r.PermissionId)
                .NotNull();
            RuleFor(r => r.PermissionId)
                .NotEmpty()
            .MustAsync(async (permission, _) => !await permissionService.ExistsByIdAsync(permission, permission))
                    .WithMessage("Similar Permission already asigned.");
        }
    }
}