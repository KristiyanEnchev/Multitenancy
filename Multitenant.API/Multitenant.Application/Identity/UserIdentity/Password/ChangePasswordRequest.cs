namespace Multitenant.Application.Identity.UserIdentity.Password
{
    using System.Text.Json.Serialization;

    using FluentValidation;

    using MediatR;

    using Multitenant.Application.Validations;
    using Multitenant.Application.Interfaces.Identity;

    public class ChangePasswordRequest : IRequest<string>
    {
        [JsonIgnore]
        public string? UserId { get; set; }
        public string Password { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
        public string ConfirmNewPassword { get; set; } = default!;
    }

    public class ChangePasswordRequestValidator : CustomValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(p => p.UserId)
                .NotEmpty().NotNull();

            RuleFor(p => p.Password)
                .NotEmpty();

            RuleFor(p => p.NewPassword)
                .NotEmpty();

            RuleFor(p => p.ConfirmNewPassword)
                .Equal(p => p.NewPassword)
                    .WithMessage("Passwords do not match.");
        }
    }

    public class ChangePasswordRequestHandler : IRequestHandler<ChangePasswordRequest, string>
    {
        private readonly IAuthService _authService;

        public ChangePasswordRequestHandler(IAuthService authService) => _authService = authService;

        public Task<string> Handle(ChangePasswordRequest request, CancellationToken cancellationToken) =>
            _authService.ChangePasswordAsync(request, request.UserId!);
    }
}