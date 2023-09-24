namespace Multitenant.Application.Identity.User.Password
{
    using FluentValidation;

    using Multitenant.Application.Validations;

    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = default!;
    }

    public class ForgotPasswordRequestValidator : CustomValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordRequestValidator() =>
            RuleFor(p => p.Email).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .EmailAddress()
                    .WithMessage("Invalid Email Address.");
    }
}