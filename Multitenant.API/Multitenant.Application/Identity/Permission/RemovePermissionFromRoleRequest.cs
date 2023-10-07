namespace Multitenant.Application.Identity.Role
{
    using FluentValidation;

    using Multitenant.Application.Validations;
    using Multitenant.Application.Interfaces.Identity;

    public class RemovePermissionFromRoleRequest
    {
        public string RoleId { get; set; } = default!;
        public int PermissionId { get; set; }
    }

    public class RemovePermissionFromRoleRequestValidator : CustomValidator<RemovePermissionFromRoleRequest>
    {
        public RemovePermissionFromRoleRequestValidator(IPermissionService permissionService)
        {
            RuleFor(r => r.RoleId)
                .NotEmpty();
            RuleFor(r => r.PermissionId)
                .NotNull();
            RuleFor(r => r.PermissionId)
                .NotEmpty()
                .MustAsync(async (permission, name, _) => !await permissionService.RoleHasPermission(permission.RoleId, permission.PermissionId))
                    .WithMessage("Role does not have this permission.");
        }
    }
}