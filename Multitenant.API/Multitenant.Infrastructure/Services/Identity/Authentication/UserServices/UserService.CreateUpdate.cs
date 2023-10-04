namespace Multitenant.Auth.UserServices
{
    using System.Security.Claims;

    using Microsoft.Identity.Web;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    using Multitenant.Domain.Events;
    using Multitenant.Application.Exceptions;
    using Multitenant.Domain.Entities.Identity;
    using Multitenant.Shared.ClaimsPrincipal;
    using Multitenant.Application.Identity.UserIdentity;

    public partial class UserService
    {
        /// <summary>
        /// This is used when authenticating with AzureAd.
        /// The local user is retrieved using the objectidentifier claim present in the ClaimsPrincipal.
        /// If no such claim is found, an InternalServerException is thrown.
        /// If no user is found with that ObjectId, a new one is created and populated with the values from the ClaimsPrincipal.
        /// If a role claim is present in the principal, and the user is not yet in that roll, then the user is added to that role.
        /// </summary>
        public async Task<string> GetOrCreateFromPrincipalAsync(ClaimsPrincipal principal)
        {
            string? objectId = principal.GetObjectId();
            if (string.IsNullOrWhiteSpace(objectId))
            {
                throw new InternalServerException("Invalid objectId");
            }

            var user = await _userManager.Users.Where(u => u.ObjectId == objectId).FirstOrDefaultAsync()
                ?? await CreateOrUpdateFromPrincipalAsync(principal);

            if (principal.FindFirstValue(ClaimTypes.Role) is string role &&
                await _roleManager.RoleExistsAsync(role) &&
                !await _userManager.IsInRoleAsync(user, role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            return user.Id;
        }

        private async Task<User> CreateOrUpdateFromPrincipalAsync(ClaimsPrincipal principal)
        {
            string? email = principal.FindFirstValue(ClaimTypes.Upn);
            string? username = principal.GetDisplayName();
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username))
            {
                throw new InternalServerException(string.Format("Username or Email not valid."));
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user is not null && !string.IsNullOrWhiteSpace(user.ObjectId))
            {
                throw new InternalServerException(string.Format($"Username - {username} is already taken."));
            }

            if (user is null)
            {
                user = await _userManager.FindByEmailAsync(email);
                if (user is not null && !string.IsNullOrWhiteSpace(user.ObjectId))
                {
                    throw new InternalServerException(string.Format($"Email - {email} is already taken."));
                }
            }

            IdentityResult? result;
            if (user is not null)
            {
                user.ObjectId = principal.GetObjectId();
                result = await _userManager.UpdateAsync(user);

                await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id));
            }
            else
            {
                user = new User
                {
                    ObjectId = principal.GetObjectId(),
                    FirstName = principal.FindFirstValue(ClaimTypes.GivenName),
                    LastName = principal.FindFirstValue(ClaimTypes.Surname),
                    Email = email,
                    NormalizedEmail = email.ToUpperInvariant(),
                    UserName = username,
                    NormalizedUserName = username.ToUpperInvariant(),
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    IsActive = true
                };
                result = await _userManager.CreateAsync(user);

                await _events.PublishAsync(new ApplicationUserCreatedEvent(user.Id));
            }

            var errors = result.Errors.Select(e => e.Description);

            if (!result.Succeeded)
            {
                throw new InternalServerException("Validation Errors Occurred.", errors.ToList());
            }

            return user;
        }

        public async Task<string> CreateAsync(CreateUserRequest request, string origin)
        {
            var checkForEmailExist = await _userManager.FindByEmailAsync(request.Email);

            if (checkForEmailExist != null)
            {
                string error = "Email already exists!";

                throw new ConflictException(error);
            }

            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.UserName,
                PhoneNumber = request.PhoneNumber,
                IsActive = true
            };

            var roleExist = await _roleManager.RoleExistsAsync(Roles.Basic);

            if (!roleExist)
            {
                string error = $"The Role:{Roles.Basic} does not exist";

                throw new NotFoundException(error);
            }

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);

                throw new InternalServerException("Validation Errors Occurred.", errors.ToList());
            }

            await _userManager.AddToRoleAsync(user, Roles.Basic);

            var messages = new List<string> { string.Format("User {0} Registered.", user.UserName) };

            if (_securitySettings.RequireConfirmedAccount && !string.IsNullOrEmpty(user.Email))
            {
                string emailVerificationUri = await _Util.GetEmailVerificationUriAsync(user, origin);

                var req = _Util.CreateRegistrationEmailRequest(user, emailVerificationUri);

                HttpResponseMessage response;

                try
                {
                    //////////////////////////////////////////!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!//////////////////////
                    response = await _emailService.SendRegistrationEmail(req);

                }
                catch (Exception ex)
                {
                    await _userManager.DeleteAsync(user);

                    throw;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = $"Somthing Went Wrong When Sending The Email To: {user.Email}";

                    string errorContent = await response.Content.ReadAsStringAsync();
                    string errorMessage = $"HTTP Error {(int)response.StatusCode}: {response.ReasonPhrase}\n{errorContent}";

                    await _userManager.DeleteAsync(user);

                    throw new CustomException(error, new List<string> { errorMessage }, response.StatusCode);
                }

                messages.Add($"Please check {user.Email} to verify your account!");

            }

            await _events.PublishAsync(new ApplicationUserCreatedEvent(user.Id));

            return string.Join(Environment.NewLine, messages);
        }

        public async Task UpdateAsync(UpdateUserRequest request, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            _ = user ?? throw new NotFoundException("User Not Found.");

            string currentImage = user.ImageUrl ?? string.Empty;
            //if (request.Image != null || request.DeleteCurrentImage)
            //{
            //    user.ImageUrl = await _fileStorage.UploadAsync<ApplicationUser>(request.Image, FileType.Image);
            //    if (request.DeleteCurrentImage && !string.IsNullOrEmpty(currentImage))
            //    {
            //        string root = Directory.GetCurrentDirectory();
            //        _fileStorage.Remove(Path.Combine(root, currentImage));
            //    }
            //}

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            string? phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (request.PhoneNumber != phoneNumber)
            {
                await _userManager.SetPhoneNumberAsync(user, request.PhoneNumber);
            }

            var result = await _userManager.UpdateAsync(user);

            var errors = result.Errors.Select(e => e.Description);

            await _signInManager.RefreshSignInAsync(user);

            await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id));

            if (!result.Succeeded)
            {
                throw new InternalServerException("Update profile failed", errors.ToList());
            }
        }
    }
}