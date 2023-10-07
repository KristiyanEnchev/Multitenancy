namespace Multitenant.Infrastructure.Services.Identity.Authentication.AuthService
{
    using System.Text;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.WebUtilities;

    using Multitenant.Application.Exceptions;

    public partial class AuthService
    {
        public async Task<string> ConfirmEmailAsync(string userId, string code, string tenant, CancellationToken cancellationToken)
        {
            _util.EnsureValidTenant();

            var user = await _userManager.Users
                .Where(u => u.Id == userId && !u.EmailConfirmed)
                .FirstOrDefaultAsync(cancellationToken);

            _ = user ?? throw new InternalServerException($"An error occurred while confirming E-Mail.");

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            return result.Succeeded
                ? string.Format($"Account Confirmed for E-Mail {user.Email}. You can now login.")
                : throw new InternalServerException(string.Format($"An error occurred while confirming {user.Email}"));
        }


        public async Task<string> ResendEmailAsync(string email, string origin)
        {
            _util.EnsureValidTenant();

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                string error = "User with that email does not exists!";

                throw new ConflictException(error);
            }

            string emailVerificationUri = await _util.GetEmailVerificationUriAsync(user, origin);
            var req = _util.CreateRegistrationEmailRequest(user, emailVerificationUri);

            HttpResponseMessage response = await emailService.SendRegistrationEmail(req);

            if (!response.IsSuccessStatusCode)
            {
                var error = $"Somthing Went Wrong When Sending The Email To: {user.Email}";

                string errorContent = await response.Content.ReadAsStringAsync();
                string errorMessage = $"HTTP Error {(int)response.StatusCode}: {response.ReasonPhrase}\n{errorContent}";
                await _userManager.DeleteAsync(user);

                throw new CustomException(error, new List<string> { errorMessage }, response.StatusCode);
            }

            return $"Email sent to {email}";
        }

        public async Task<string> ConfirmPhoneNumberAsync(string userId, string code)
        {
            _util.EnsureValidTenant();

            var user = await _userManager.FindByIdAsync(userId);

            _ = user ?? throw new InternalServerException($"An error occurred while confirming Mobile Phone.");
            if (string.IsNullOrEmpty(user.PhoneNumber)) throw new InternalServerException($"An error occurred while confirming Mobile Phone.");

            var result = await _userManager.ChangePhoneNumberAsync(user, user.PhoneNumber, code);

            return result.Succeeded
                ? user.PhoneNumberConfirmed
                    ? string.Format($"Account Confirmed for Phone Number {user.PhoneNumber}. You can now use the /api/tokens endpoint to generate JWT.")
                    : string.Format($"Account Confirmed for Phone Number {user.PhoneNumber}. You should confirm your E-mail before using the /api/tokens endpoint to generate JWT.")
                : throw new InternalServerException(string.Format($"An error occurred while confirming {user.PhoneNumber}"));
        }
    }
}