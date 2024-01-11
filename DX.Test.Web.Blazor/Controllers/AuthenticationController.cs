using DX.Blazor.Identity.Server.Controllers;
using DX.Test.Web.Blazor.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DX.Blazor.Identity.Models;
using DX.Blazor.Identity;

namespace DX.Test.Web.Blazor.Controllers
{
    // DX.Blazor.Identity.Server authentication controller
    [Route("api/accounts")]
    public class AuthenticationController : AuthenticationControllerBase<ApplicationUser, RegisterUserModel>
    {
        public AuthenticationController(UserManager<ApplicationUser> userManager, 
                ITokenService<string, ApplicationUser> tokenService,
                SignInManager<ApplicationUser> signInManager, IDataProtectionProvider dataProtectionProvider, 
                ILogger<AuthenticationControllerBase<string, ApplicationUser, RegisterUserModel>> logger, IConfiguration configuration) 
            : base(userManager, tokenService, signInManager, dataProtectionProvider, logger, configuration)
        {

        }

        protected override ApplicationUser CreateFromRegistrationModel(RegisterUserModel registrationModel)
        {
            return new ApplicationUser
            {
                UserName = registrationModel.Email,
                Email = registrationModel.Email,
                BirthDate = registrationModel.BirthDate,
                Street = registrationModel.Street,
                HouseNo = registrationModel.HouseNo,
                HouseNoSuffix = registrationModel.HouseNoSuffix,
                ZipCode = registrationModel.ZipCode,
                City = registrationModel.City,
                State = registrationModel.State,
                Country = registrationModel.Country
            };
        }

        protected override string GetPassword(RegisterUserModel registrationModel)
        {
            return registrationModel.Password;
        }
    }
}
