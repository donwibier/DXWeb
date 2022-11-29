using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DX.Blazor.Identity.Server.Services
{
    public class TokenAccessService 
    {
        readonly IHttpContextAccessor contextAccessor;

        public TokenAccessService(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        public string GetToken()
        {
            return contextAccessor.HttpContext?.GetTokenAsync("access_token").GetAwaiter().GetResult() ?? string.Empty;
        }

        //public string GetCurrentUserName()
        //{
        //    return contextAccessor.HttpContext.User.Claims.Single(x => x.Type == ClaimTypes.Name).Value;
        //}
    }
}
