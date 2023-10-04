namespace Multitenant.Infrastructure.Services.Identity.Authentication.AuthService
{
    using Microsoft.AspNetCore.Identity;

    using Multitenant.Shared;
    using Multitenant.Models.Security;
    using Multitenant.Shared.Constants.Multitenancy;
    using Multitenant.Application.Exceptions;
    using Multitenant.Application.Identity.Token;
    using Multitenant.Application.Interfaces.Identity;
    using Multitenant.Domain.Entities.Identity;
    using Multitenant.Application.Identity.UserIdentity;
    using Multitenant.Shared.ClaimsPrincipal;
    using Multitenant.Application.Interfaces.Mailing;
    using Multitenant.Infrastructure.Services.Identity.Authentication.Util;
    using Multitenant.Application.Events;
    using Multitenant.Domain.Events;
    using Microsoft.Extensions.Options;

    public partial class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<UserRole> _roleManager;
        private readonly SecuritySettings _securitySettings;
        private readonly MultiTenantInfo? _currentTenant;
        private readonly ITokenService? tokenService;
        private readonly IEmailService emailService;
        private readonly IEventPublisher _events;
        private readonly IUtil _util;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IOptions<SecuritySettings> securitySettings,
            MultiTenantInfo? currentTenant,
            RoleManager<UserRole> roleManager,
            ITokenService? tokenService,
            IEmailService emailService,
            IUtil util,
            IEventPublisher events)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _securitySettings = securitySettings.Value;
            _currentTenant = currentTenant;
            this.tokenService = tokenService;
            this.emailService = emailService;
            _util = util;
            _events = events;
        }

        public async Task<string> RegisterAsync(CreateUserRequest request, string origin)
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
                string emailVerificationUri = await _util.GetEmailVerificationUriAsync(user, origin);

                var req = _util.CreateRegistrationEmailRequest(user, emailVerificationUri);

                HttpResponseMessage response;

                try
                {
                    response = await emailService.SendRegistrationEmail(req);

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