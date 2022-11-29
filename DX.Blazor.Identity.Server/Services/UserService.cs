using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;

namespace DX.Blazor.Identity.Server.Services
{
    public interface IUserService
    {
        string GetCurrentUserId();
        string GetCurrentUserName();
    }

    public class UserService : IUserService
    {
        readonly IHttpContextAccessor contextAccessor;

        public UserService(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        public string GetCurrentUserId()
        {
            return contextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid)!.Value ?? string.Empty;
        }

        public string GetCurrentUserName()
        {
            return contextAccessor.HttpContext?.User.Claims.Single(x => x.Type == ClaimTypes.Name).Value ?? string.Empty;
        }
    }
}
