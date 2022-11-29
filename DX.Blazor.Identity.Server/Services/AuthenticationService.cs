using DX.Blazor.Identity.Models;
using DX.Data.Xpo.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DX.Blazor.Identity.Server.Services
{
    public class AuthenticationService<TUser>
         : AuthenticationService<TUser, RegistrationModel>, IAuthService
         where TUser : class, IXPUser<string>, new()
    {
        public AuthenticationService(UserManager<TUser> userManager, 
            IDataProtectionProvider dataProtectionProvider,
            NavigationManager navigationManager, 
            HttpClient httpClient, 
            ILogger<AuthenticationService<string, TUser, RegistrationModel, AuthenticationModel>> logger) 
            : base(userManager, dataProtectionProvider, navigationManager, httpClient, logger)
        {

        }
    }

    public class AuthenticationService<TUser, TRegistrationModel>
         : AuthenticationService<string, TUser, TRegistrationModel, AuthenticationModel>, IAuthService<TRegistrationModel, AuthenticationModel>
         where TUser : class, IXPUser<string>, new()
         where TRegistrationModel : class, new()
    {
        public AuthenticationService(UserManager<TUser> userManager, 
            IDataProtectionProvider dataProtectionProvider, 
            NavigationManager navigationManager, 
            HttpClient httpClient, 
            ILogger<AuthenticationService<string, TUser, TRegistrationModel, AuthenticationModel>> logger) 
            : base(userManager, dataProtectionProvider, navigationManager, httpClient, logger)
        {

        }

        protected override string GetEmail(AuthenticationModel model) => model.Email;
        protected override string GetPassword(AuthenticationModel model) => model.Password;
        protected override bool GetRememberMe(AuthenticationModel model) => model.RememberMe;
        protected override string GetReturnUrl(AuthenticationModel model) => model.ReturnUrl;
    }

    public abstract class AuthenticationService<TKey, TUser, TRegistrationModel>
         : AuthenticationService<TKey, TUser, TRegistrationModel, AuthenticationModel>, IAuthService<TRegistrationModel, AuthenticationModel>
         where TKey : IEquatable<TKey>
         where TUser : class, IXPUser<TKey>, new()
         where TRegistrationModel : class, new()
    {
        public AuthenticationService(UserManager<TUser> userManager, 
            IDataProtectionProvider dataProtectionProvider, 
            NavigationManager navigationManager, 
            HttpClient httpClient, 
            ILogger<AuthenticationService<TKey, TUser, TRegistrationModel, AuthenticationModel>> logger) 
            : base(userManager, dataProtectionProvider, navigationManager, httpClient, logger)
        {

        }
    }
    /// <summary>
    /// Blazor Server AuthenticationService
    /// </summary>
    public abstract class AuthenticationService<TKey, TUser, TRegistrationModel, TAuthenticationModel> 
            : IAuthService<TRegistrationModel, TAuthenticationModel>
        where TKey : IEquatable<TKey>
        where TUser: class, IXPUser<TKey>, new()
        where TRegistrationModel : class, new()
        where TAuthenticationModel : class, new()
    {
        const string registerEndpoint = "/api/Accounts/Registration";
        const string loginEndpoint = "/api/Accounts/Login";
        const string logoutEndpoint = "/api/Accounts/Logout";

        readonly UserManager<TUser> userManager;
        readonly IDataProtectionProvider dataProtectionProvider;
        readonly NavigationManager navigationManager;
        readonly ILogger<AuthenticationService<TKey, TUser, TRegistrationModel, TAuthenticationModel>> logger;
        readonly HttpClient httpClient;

        public AuthenticationService(UserManager<TUser> userManager,
            IDataProtectionProvider dataProtectionProvider, NavigationManager navigationManager,
            HttpClient httpClient,
            ILogger<AuthenticationService<TKey, TUser, TRegistrationModel, TAuthenticationModel>> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
            this.navigationManager = navigationManager;
            this.userManager = userManager;
            this.dataProtectionProvider = dataProtectionProvider;
        }

        protected abstract bool GetRememberMe(TAuthenticationModel model);
        protected abstract string GetEmail(TAuthenticationModel model);
        protected abstract string GetPassword(TAuthenticationModel model);
        protected abstract string GetReturnUrl(TAuthenticationModel model);

        public async Task<AuthResponseModel> Login(TAuthenticationModel userForAuthentication)
        {
            var identityUser = (await userManager.FindByEmailAsync(GetEmail(userForAuthentication))) ?? default!;

            if (await userManager.CheckPasswordAsync(identityUser, GetPassword(userForAuthentication)) == true)
            {
                var token = await userManager.GenerateUserTokenAsync(identityUser, TokenOptions.DefaultProvider, "Login");
                var data = $"{identityUser.Id}|{token}|{GetRememberMe(userForAuthentication)}|{GetReturnUrl(userForAuthentication) ?? navigationManager.ToAbsoluteUri("/").ToString()}";
                var protector = dataProtectionProvider.CreateProtector("Login");
                var protectedData = protector.Protect(data);
                navigationManager.NavigateTo($"{loginEndpoint}?token=" + protectedData, true);
                return new AuthResponseModel { IsAuthSuccessful = true };
            }
            return new AuthResponseModel { IsAuthSuccessful = false, ErrorMessage = "Invalid login attempt." };
        }

        public Task Logout()
        {
            navigationManager.NavigateTo(logoutEndpoint, true);
            return Task.CompletedTask;
        }

        public Task<string> RefreshToken()
        {
            return Task.FromResult(string.Empty);
        }

        public async Task<RegistrationResponseModel> RegisterUser(TRegistrationModel userForRegistration)
        {
            var response = await httpClient.PostAsJsonAsync(navigationManager.ToAbsoluteUri(registerEndpoint), userForRegistration);
            var result = await response.Content.ReadFromJsonAsync<RegistrationResponseModel>();

            if (result != null && result.IsSuccessfulRegistration)
            {
                logger.LogInformation("User created a new account with password.");
                //TODO: Confirmation
                //var user = await userManager.FindByNameAsync(userForRegistration.Email);
                //if (user != null)
                //{
                //    var userId = await userManager.GetUserIdAsync(user);
                //    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                //    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                //    //var callbackUrl = navigationManager.ToAbsoluteUri($"/Identity/Account/ConfirmEmail?userId={userId}&code={code}&returnUrl={redirectUrl}").ToString();
                //}
                //else
                //{
                //    logger.LogInformation("Created user could not be found.");
                //}
            }
            return result!;
        }

    }
}
