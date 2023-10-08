namespace Multitenant.Application.Identity.UserIdentity.Password
{
    using FluentValidation;

    using MediatR;

    using Multitenant.Application.Interfaces.Identity;
    using Multitenant.Application.Validations;

    /// <summary>
    /// Represents a change password request.
    /// </summary>
    public class ChangeUserPasswordRequest : IRequest<string>
    {
        /// <summary>
        /// This property will not appear in Swagger documentation.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// The user's current password.
        /// </summary>
        public string? Password { get; set; } = default!;

        /// <summary>
        /// The new password.
        /// </summary>
        public string NewPassword { get; set; } = default!;

        /// <summary>
        /// The confirmation of the new password.
        /// </summary>
        public string ConfirmNewPassword { get; set; } = default!;
    }

    public class ChangeUserPasswordRequestValidator : CustomValidator<ChangeUserPasswordRequest>
    {
        public ChangeUserPasswordRequestValidator()
        {
            RuleFor(p => p.UserId)
                .NotNull()
                .NotEmpty()
                .WithMessage("UserId is required."); ;

            RuleFor(p => p.NewPassword)
                .NotEmpty();

            RuleFor(p => p.ConfirmNewPassword)
                .Equal(p => p.NewPassword)
                    .WithMessage("Passwords do not match.");
        }
    }

    public class ChangeUserPasswordRequestHandler : IRequestHandler<ChangeUserPasswordRequest, string>
    {
        private readonly IUserService _userService;

        public ChangeUserPasswordRequestHandler(IUserService userService) => _userService = userService;

        public Task<string> Handle(ChangeUserPasswordRequest request, CancellationToken cancellationToken) =>
            _userService.ChangePasswordAsync(request);
    }
}