using DX.Blazor.Identity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DX.Blazor.Identity
{
    public interface IAuthService<TRegistrationModel, TAuthenticationModel>
        where TRegistrationModel : class, new()
        where TAuthenticationModel : class, new()
    {
        Task<RegistrationResponseModel> RegisterUser(TRegistrationModel userForRegistration);
        Task<AuthResponseModel> Login(TAuthenticationModel userForAuthentication);
        Task Logout();
        Task<string> RefreshToken();
    }

    public interface IAuthService<TRegistrationModel> : IAuthService<TRegistrationModel, AuthenticationModel>
        where TRegistrationModel : class, new()
    {
    }
    public interface IAuthService : IAuthService<RegistrationModel, AuthenticationModel>        
    {
    }
}
