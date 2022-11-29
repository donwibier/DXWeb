using DX.Blazor.Identity.Models;
using DX.Data.Xpo.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DX.Blazor.Identity.Wasm.Controllers
{
    //[Route("api/token")]
    //[ApiController]
    public class TokenControllerBase<TKey, TUser> : ControllerBase
        where TKey : IEquatable<TKey>
        where TUser : class, IXPUser<TKey>, new()
    {
        private readonly UserManager<TUser> _userManager;
        private readonly ITokenService<TKey, TUser> _tokenService;

        public TokenControllerBase(UserManager<TUser> userManager, ITokenService<TKey, TUser> tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [HttpPost]
        [Route("refresh")]
        public virtual async Task<IActionResult> Refresh([FromBody] RefreshTokenModel tokenModel)
        {
            if (tokenModel is null)
            {
                return BadRequest(new AuthResponseModel { IsAuthSuccessful = false, ErrorMessage = "Invalid client request" });
            }

            var principal = _tokenService.GetPrincipalFromExpiredToken(tokenModel.Token);
            var username = principal?.Identity?.Name ?? string.Empty;

            var user = await _userManager.FindByEmailAsync(username);
            if (user == null || user.RefreshToken != tokenModel.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
                return BadRequest(new AuthResponseModel { IsAuthSuccessful = false, ErrorMessage = "Invalid client request" });

            var signingCredentials = _tokenService.GetSigningCredentials();
            var claims = await _tokenService.GetClaims(user);
            var tokenOptions = _tokenService.GenerateTokenOptions(signingCredentials, claims);
            var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            user.RefreshToken = _tokenService.GenerateRefreshToken();

            await _userManager.UpdateAsync(user);

            return Ok(new AuthResponseModel { Token = token, RefreshToken = user.RefreshToken, IsAuthSuccessful = true });
        }
    }
}
