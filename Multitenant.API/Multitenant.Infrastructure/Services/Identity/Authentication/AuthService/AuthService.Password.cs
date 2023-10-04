namespace Multitenant.Infrastructure.Services.Identity.Authentication.AuthService
{
    using Microsoft.AspNetCore.WebUtilities;

    using Multitenant.Application.Exceptions;
    using Multitenant.Application.Identity.UserIdentity.Password;

    public partial class AuthService
    {
        public async Task<string> ForgotPasswordAsync(ForgotPasswordRequest request, string origin)
        {
            _util.EnsureValidTenant();

            var user = await _userManager.FindByEmailAsync(request.Email.Normalize());
            if (user is null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                // Don't reveal that the user does not exist or is not confirmed
                throw new InternalServerException("An Error has occurred!");
            }

            string code = await _userManager.GeneratePasswordResetTokenAsync(user);

            const string route = "api/auth/reset-password";

            var endpointUri = new Uri(string.Concat($"{origin}/", route));

            string passwordResetUrl = QueryHelpers.AddQueryString(endpointUri.ToString(), "Token", code);

            var messages = new List<string> { string.Format("User: {0}, Password Reset Mail has been sent to your Email: {1}", user.UserName, user.Email) };

            var eMailModel = _util.CreateRessetPasswordEmailRequest(user, code, endpointUri.ToString(), passwordResetUrl);

            var response = await emailService.SendPasswordResetnEmail(eMailModel);

            if (!response.IsSuccessStatusCode)
            {
                var error = $"Somthing Went Wrong When Sending The Email To: {user.Email}";

                string errorContent = await response.Content.ReadAsStringAsync();
                string errorMessage = $"HTTP Error {(int)response.StatusCode}: {response.ReasonPhrase}\n{errorContent}";

                throw new CustomException(error, new List<string> { errorMessage }, response.StatusCode);
            }

            messages.Add($"Please check {user.Email} to resset you password!");

            var message = string.Join(Environment.NewLine, messages);

            return message;
        }

        public async Task<string> ResetPasswordAsync(ResetPasswordRequest request)
        {
            _util.EnsureValidTenant();

            var user = await _userManager.FindByEmailAsync(request.Email?.Normalize()!);

            // Don't reveal that the user does not exist
            _ = user ?? throw new InternalServerException("An Error has occurred!");

            var result = await _userManager.ResetPasswordAsync(user, request.Token!, request.Password!);

            var errors = result.Errors.Select(e => e.Description);

            return result.Succeeded
                ? "Password Reset Successful!"
                : throw new InternalServerException("An Error has occurred!", errors.ToList());
        }

        public async Task<string> ChangePasswordAsync(ChangePasswordRequest model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            _ = user ?? throw new NotFoundException("User Not Found.");
            if (!user.EmailConfirmed) throw new NotFoundException("User Not Found.");

            var result = await _userManager.ChangePasswordAsync(user, model.Password, model.NewPassword);

            var errors = result.Errors.Select(e => e.Description);

            if (!result.Succeeded)
            {
                throw new InternalServerException("Change password failed", errors.ToList());
            }

            return "Password Reset Successful!";
        }
    }
}