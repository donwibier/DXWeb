using DX.Blazor.Identity.Models;
using DX.Blazor.Identity.Server.Services;
using DX.Test.Web.Blazor.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace DX.Test.Web.Blazor.Services
{
    public class AuthService : AuthenticationService<ApplicationUser, RegisterUserModel>
    {
        public AuthService(UserManager<ApplicationUser> userManager, 
            IDataProtectionProvider dataProtectionProvider, 
            NavigationManager navigationManager, 
            HttpClient httpClient, 
            ILogger<AuthenticationService<string, ApplicationUser, RegisterUserModel, AuthenticationModel>> logger) 
            : base(userManager, dataProtectionProvider, navigationManager, httpClient, logger)
        {

        }
    }
}
