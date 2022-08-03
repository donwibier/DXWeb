using Microsoft.AspNetCore.Mvc;
using DX.Data.Xpo.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using DX.Blazor.Identity.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authorization;

namespace DX.Blazor.Identity.Server.Controllers
{
    public class AuthenticationControllerBase<TUser> : AuthenticationControllerBase<string, TUser, RegistrationModel>
        where TUser : class, IXPUser<string>, new()
    {
        public AuthenticationControllerBase(UserManager<TUser> userManager, 
            SignInManager<TUser> signInManager, 
            IDataProtectionProvider dataProtectionProvider, 
            ILogger<AuthenticationControllerBase<string, TUser, RegistrationModel>> logger, 
            IConfiguration configuration) 
            : base(userManager, signInManager, dataProtectionProvider, logger, configuration)
        {

        }

        protected override TUser CreateFromRegistrationModel(RegistrationModel registrationModel)
        {
            return new TUser() { UserName = registrationModel.Email, Email = registrationModel.Email };
        }

        protected override string GetPassword(RegistrationModel registrationModel) => registrationModel.Password;
    }

    public abstract class AuthenticationControllerBase<TUser, TRegistrationModel> : AuthenticationControllerBase<string, TUser, TRegistrationModel>
        where TUser : class, IXPUser<string>, new()
        where TRegistrationModel : class, new()
    {
        public AuthenticationControllerBase(UserManager<TUser> userManager, 
            SignInManager<TUser> signInManager, 
            IDataProtectionProvider dataProtectionProvider, 
            ILogger<AuthenticationControllerBase<string, TUser, TRegistrationModel>> logger, 
            IConfiguration configuration) 
            : base(userManager, signInManager, dataProtectionProvider, logger, configuration)
        {

        }
    }


    public abstract class AuthenticationControllerBase<TKey, TUser, TRegistrationModel> : Controller
        where TKey : IEquatable<TKey>
        where TUser : class, IXPUser<TKey>, new()
        where TRegistrationModel: class, new()
    {
        private readonly UserManager<TUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IConfigurationSection _jwtSettings;

        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly SignInManager<TUser> _signInManager;
        readonly ILogger<AuthenticationControllerBase<TKey, TUser, TRegistrationModel>> _logger;


        public AuthenticationControllerBase(UserManager<TUser> userManager,
            SignInManager<TUser> signInManager,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<AuthenticationControllerBase<TKey, TUser, TRegistrationModel>> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _dataProtectionProvider = dataProtectionProvider;
            _configuration = configuration;
            _jwtSettings = _configuration.GetSection("JwtSettings");
        }

        protected abstract TUser CreateFromRegistrationModel(TRegistrationModel registrationModel);
        protected abstract string GetPassword(TRegistrationModel registrationModel);

        [HttpPost("Registration")]
        public virtual async Task<IActionResult> RegisterUser([FromBody] TRegistrationModel registrationModel)
        {
            if (registrationModel == null || !ModelState.IsValid)
                return BadRequest();
            
            var user = CreateFromRegistrationModel(registrationModel);
            var result = await _userManager.CreateAsync(user, GetPassword(registrationModel));
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);

                return BadRequest(new RegistrationResponseModel { Errors = errors });
            }
            else
            {
                _logger.LogInformation($"User {user.UserName} created a new account with password.");
            }
            //TODO: check for email confirmation etc.

            return Ok(new RegistrationResponseModel { IsSuccessfulRegistration = true }); // StatusCode(201);
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public virtual async Task<IActionResult> Login([FromBody] AuthenticationModel userForAuthentication)
        {
            var user = await _userManager.FindByNameAsync(userForAuthentication.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, userForAuthentication.Password))
                return Unauthorized(new AuthResponseModel { ErrorMessage = "Invalid Authentication" });

            var signingCredentials = GetSigningCredentials();
            var claims = await GetClaims(user);
            var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
            var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return Ok(new AuthResponseModel { IsAuthSuccessful = true, Token = token });
        }
        [HttpGet("Login")]
        [AllowAnonymous]
        public virtual async Task<IActionResult> Login(string token)
        {
            var dataProtector = _dataProtectionProvider.CreateProtector("Login");
            var data = dataProtector.Unprotect(token);
            var parts = data.Split('|');
            var returnUrl = parts[3];
            {
                if (string.IsNullOrWhiteSpace(returnUrl))
                    returnUrl = "/";
            }
            var identityUser = await _userManager.FindByIdAsync(parts[0]);
            if (identityUser == null)
            {
                return Unauthorized();
            }
            var isTokenValid = await _userManager.VerifyUserTokenAsync(identityUser, TokenOptions.DefaultProvider, "Login", parts[1]);
            if (isTokenValid)
            {
                var isPersistent = bool.Parse(parts[2]);
                await _userManager.ResetAccessFailedCountAsync(identityUser);
                await _signInManager.SignInAsync(identityUser, isPersistent);

                //TODO: Check for confirmation email etc.

                //await emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                //$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                //if (userManager.Options.SignIn.RequireConfirmedAccount)
                //{
                //    navigationManager.NavigateTo($"RegisterConfirmation?email?={userForRegistration.Email}&returnUrl={redirectUrl}");
                //}
                //else
                //{
                //    await signInManager.SignInAsync(user, isPersistent: false);
                //    navigationManager.NavigateTo(redirectUrl);
                //}

                if (!string.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                else
                    return Ok(new AuthResponseModel { IsAuthSuccessful = true });
            }

            return Unauthorized();
        }

        [HttpGet("ExternalLogins")]
        [AllowAnonymous]
        public virtual async Task<IActionResult> ExternalLogins()
        {
            var result = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            return Ok(result);
        }

        [HttpGet("LogOut")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Redirect("/");
        }


        private SigningCredentials GetSigningCredentials()
        {
            var key = Encoding.UTF8.GetBytes(_jwtSettings.GetSection("securityKey").Value);
            var secret = new SymmetricSecurityKey(key);

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        protected virtual async Task<List<Claim>> GetClaims(TUser user)
        {
            //ClaimsIdentity
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email)
            };
            //claims.AddRange(await AssignClaims(user));

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }        

        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var tokenOptions = new JwtSecurityToken(
                issuer: _jwtSettings.GetSection("validIssuer").Value,
                audience: _jwtSettings.GetSection("validAudience").Value,
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_jwtSettings.GetSection("expiryInMinutes").Value)),
                signingCredentials: signingCredentials);

            return tokenOptions;
        }

    }
}
