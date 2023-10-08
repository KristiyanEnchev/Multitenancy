namespace Multitenant.Application.Identity.Role
{
    using FluentValidation;

    using Multitenant.Application.Validations;
    using Multitenant.Application.Interfaces.Identity;
    using MediatR;

    public class RemovePermissionFromRoleRequest : IRequest<string>
    {
        public string RoleId { get; set; } = default!;
        public int PermissionId { get; set; }

        public RemovePermissionFromRoleRequest(string roleId, int permissionId)
        {
            RoleId = roleId;
            PermissionId = permissionId;
        }
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
                .MustAsync(async (permission, name, _) => await permissionService.RoleHasPermission(permission.RoleId, permission.PermissionId))
                    .WithMessage("Role does not have this permission.");
        }
    }

    public class RemovePermissionFromRoleRequestHandler : IRequestHandler<RemovePermissionFromRoleRequest, string>
    {
        private readonly IPermissionService _permissionService;

        public RemovePermissionFromRoleRequestHandler(IPermissionService permissionService) => _permissionService = permissionService;

        public Task<string> Handle(RemovePermissionFromRoleRequest request, CancellationToken cancellationToken) =>
            _permissionService.RemovePermissionFromRoleAsync(request);
    }
}