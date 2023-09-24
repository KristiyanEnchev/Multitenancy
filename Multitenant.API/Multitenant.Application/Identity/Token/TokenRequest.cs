namespace Multitenant.Application.Identity.Token
{
    using FluentValidation;

    using Multitenant.Application.Validations;

    public record TokenRequest(string Email, string Password);

    public class TokenRequestValidator : CustomValidator<TokenRequest>
    {
        public TokenRequestValidator()
        {
            RuleFor(p => p.Email).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .EmailAddress()
                    .WithMessage("Invalid Email Address.");

            RuleFor(p => p.Password).Cascade(CascadeMode.Stop)
                .NotEmpty();
        }
    }
}