using DX.Data.Xpo.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;


//using DX.Data.;

namespace DX.Blazor.Identity
{
    public interface ITokenService<TKey, TUser>
        where TKey: IEquatable<TKey>
        where TUser: IdentityUser<TKey>, IIdentityRefreshToken
    {
        SigningCredentials GetSigningCredentials();
        Task<List<Claim>> GetClaims(TUser user);
        JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
