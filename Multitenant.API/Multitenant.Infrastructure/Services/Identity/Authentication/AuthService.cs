namespace Multitenant.Infrastructure.Services.Identity.Authentication
{
    using Microsoft.AspNetCore.Identity;

    using Multitenant.Shared;
    using Multitenant.Shared.Constants.Multitenancy;
    using Multitenant.Models.Security;
    using Multitenant.Application.Exceptions;
    using Multitenant.Application.Identity.Token;
    using Multitenant.Application.Interfaces.Identity;
    using Multitenant.Domain.Entities.Identity;
    using Multitenant.Application.Identity.UserIdentity;
    using Multitenant.Domain.Events;
    using Multitenant.Shared.Events;
    using Multitenant.Shared.ClaimsPrincipal;
    using Microsoft.AspNetCore.WebUtilities;
    using Multitenant.Shared.Persistance;
    using System.Text;
    using Multitenant.Application.Interfaces.Mailing;
    using Multitenant.Models.Mailing;

    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<UserRole> _roleManager;
        //private readonly SecuritySettings _securitySettings;
        private readonly MultiTenantInfo? _currentTenant;
        private readonly ITokenService? tokenService;
        private readonly IEmailService emailService;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            //SecuritySettings securitySettings,
            MultiTenantInfo? currentTenant,
            ITokenService? tokenService,
            RoleManager<UserRole> roleManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            //_securitySettings = securitySettings;
            _currentTenant = currentTenant;
            this.tokenService = tokenService;
            this.emailService = emailService;
        }

        public async Task<TokenResponse> LoginAsync(TokenRequest request, string ipAddress, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_currentTenant?.Id)
                || await _userManager.FindByEmailAsync(request.Email.Trim().Normalize()) is not { } user
                || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                throw new UnauthorizedException($"Authentication Failed.");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedException($"User Not Active. Please contact the administrator.");
            }

            //if (_securitySettings.RequireConfirmedAccount && !user.EmailConfirmed)
            //{
            //    throw new UnauthorizedException($"E-Mail not confirmed.");
            //}

            if (_currentTenant.Id != MultitenancyConstants.Root.Id)
            {
                if (!_currentTenant.IsActive)
                {
                    throw new UnauthorizedException($"Tenant is not Active. Please contact the Application Administrator.");
                }

                if (DateTime.UtcNow > _currentTenant.ValidUpto)
                {
                    throw new UnauthorizedException($"Tenant Validity Has Expired. Please contact the Application Administrator.");
                }
            }

            SignInResult signInResult = await _signInManager.PasswordSignInAsync(user, request.Password, false, lockoutOnFailure: false);

            if (!signInResult.Succeeded)
            {
                throw new UnauthorizedException($"User can't signin with this credentials.");
            }

            if (tokenService is null) throw new Exception("Token Generation failure");

            return await tokenService.GenerateTokensAndUpdateUser(user, ipAddress);
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

            string emailVerificationUri = await GetEmailVerificationUriAsync(user, origin);
            var req = new EmailRequest
            {
                From = "Alabala@noreply.com",
                To = "kristiqnenchevv@gmail.com",
                TemplateName = "email-confirmation",
                TemplateDataList = new List<TemplateData>
                {
                    new TemplateData
                    {
                        Key = "UserName",
                        Value = user.UserName
                    },
                    new TemplateData
                    {
                        Key = "Email",
                        Value = user.Email
                    },
                     new TemplateData
                    {
                        Key = "Url",
                        Value = emailVerificationUri
                    }
                }
            };
            var isSuccessfull = await emailService.SendRegistrationEmail(req);
            //var isSuccessfull = await emailService.SendRegistrationEmail(user.Email, user.UserName, emailVerificationUri);

            if (isSuccessfull && isSuccessfull != null)
            {
                messages.Add($"Please check {user.Email} to verify your account!");
            }



            //if (_securitySettings.RequireConfirmedAccount && !string.IsNullOrEmpty(user.Email))
            //{
            //    // send verification email
            //    string emailVerificationUri = await GetEmailVerificationUriAsync(user, origin);
            //    RegisterUserEmailModel eMailModel = new RegisterUserEmailModel()
            //    {
            //        Email = user.Email,
            //        UserName = user.UserName,
            //        Url = emailVerificationUri
            //    };
            //    var mailRequest = new MailRequest(
            //        new List<string> { user.Email },
            //        _t["Confirm Registration"],
            //        _templateService.GenerateEmailTemplate("email-confirmation", eMailModel));
            //    _jobService.Enqueue(() => _mailService.SendAsync(mailRequest, CancellationToken.None));
            //    messages.Add(_t[$"Please check {user.Email} to verify your account!"]);
            //}

            //await _events.PublishAsync(new ApplicationUserCreatedEvent(user.Id));

            return string.Join(Environment.NewLine, messages);
        }

        private async Task<string> GetEmailVerificationUriAsync(User user, string origin)
        {
            //string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            //const string route = "api/Auth/ConfirmEmailAsync/";

            //var endpointUri = new Uri(string.Concat($"{origin}/", route));

            //string verificationUri = QueryHelpers.AddQueryString(endpointUri.ToString(), QueryStringKeys.UserId, user.Id.ToString());

            //verificationUri = QueryHelpers.AddQueryString(verificationUri, QueryStringKeys.Code, code);

            //return verificationUri;

            EnsureValidTenant();

            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            const string route = "api/identity/confirm-email/";
            var endpointUri = new Uri(string.Concat($"{origin}/", route));
            string verificationUri = QueryHelpers.AddQueryString(endpointUri.ToString(), QueryStringKeys.UserId, user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri, QueryStringKeys.Code, code);
            verificationUri = QueryHelpers.AddQueryString(verificationUri, MultitenancyConstants.TenantIdName, _currentTenant.Id!);
            return verificationUri;
        }

        private void EnsureValidTenant()
        {
            if (string.IsNullOrWhiteSpace(_currentTenant?.Id))
            {
                throw new UnauthorizedException("Invalid Tenant.");
            }
        }

        public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress)
        {
            if (tokenService is null) throw new Exception("Token Generation failure");

            return await tokenService.RefreshTokenAsync(request, ipAddress);
        }

        public async Task<bool> LogoutAsync(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user != null)
            {
                await _userManager.RemoveAuthenticationTokenAsync(user, "MultiAuth", "RefreshToken");
            }

            await _signInManager.SignOutAsync();

            return true;
        }
    }
}