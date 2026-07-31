# DX.Blazor.Identity

Shared authentication contracts and models used by both `DX.Blazor.Identity.Server` and `DX.Blazor.Identity.Wasm` — install this package (it comes in transitively with either of those) to get the `IAuthService`/`ITokenService` interfaces and the request/response models your Blazor components bind to. This package alone doesn't implement anything; it's the shared "wire format" both sides agree on.

**Target frameworks:** net9.0, net10.0
**DevExpress dependency:** transitive, via `DX.Data.Xpo.Identity` (`26.1.*`)

## Install

Normally installed transitively via `DX.Blazor.Identity.Server` or `DX.Blazor.Identity.Wasm`:

```
dotnet add package DX.Blazor.Identity.Server   # Blazor Server / hosted WASM API project
dotnet add package DX.Blazor.Identity.Wasm     # Blazor WASM client project
```

## IAuthService (IAuthService.cs)

```cs
public interface IAuthService<TRegistrationModel, TAuthenticationModel>
{
    Task<RegistrationResponseModel> RegisterUser(TRegistrationModel userForRegistration);
    Task<AuthResponseModel> Login(TAuthenticationModel userForAuthentication);
    Task Logout();
    Task<string> RefreshToken();
}

public interface IAuthService<TRegistrationModel> : IAuthService<TRegistrationModel, AuthenticationModel> { }
public interface IAuthService : IAuthService<RegistrationModel, AuthenticationModel> { }
```

This is what your Blazor components (Login.razor, Register.razor, ...) inject and call — `AuthenticationService<...>` in `DX.Blazor.Identity.Server` (redirect-based Server flow) and `DX.Blazor.Identity.Wasm.Services.AuthenticationService` (token/localStorage-based WASM flow) are the two concrete implementations, registered under the same `IAuthService` interface so your Razor markup doesn't need to know or care which hosting model it's running under.

## ITokenService<TKey, TUser> (ITokenService.cs)

```cs
public interface ITokenService<TKey, TUser> where TUser : IdentityUser<TKey>, IIdentityRefreshToken
{
    SigningCredentials GetSigningCredentials();
    Task<List<Claim>> GetClaims(TUser user);
    JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
```

Implemented by `DX.Blazor.Identity.Wasm.Services.TokenService<TKey, TUser>` — reads `JwtSettings:securityKey`/`validIssuer`/`validAudience`/`expiryInMinutes` from `IConfiguration` to mint and validate JWTs. The `IIdentityRefreshToken` constraint on `TUser` (defined in `DX.Data`) means your `ApplicationUser` needs `RefreshToken`/`RefreshTokenExpiryTime` properties — `DX.Data.Xpo.Identity`'s `IXPUser<TKey>` already has these, so the default XPO-backed user model works out of the box.

## Models (Models/AuthenticationModels.cs)

```cs
public class RegistrationModel { string Email; string Password; string ConfirmPassword; string ReturnUrl; } // + DataAnnotations
public class RegistrationResponseModel { bool IsSuccessfulRegistration; IEnumerable<string> Errors; }
public class AuthenticationModel { string Email; string Password; bool RememberMe; string ReturnUrl; }
public class AuthResponseModel { bool IsAuthSuccessful; string ErrorMessage; string Token; string RefreshToken; }
public class RefreshTokenModel { string Token; string RefreshToken; }
```

These are the DTOs that cross the wire between the WASM client and the server API (`Accounts/Registration`, `Accounts/Login`, `token/refresh`) — see `DX.Blazor.Identity.Wasm`'s `AuthenticationService` for the client-side HTTP calls and `DX.Blazor.Identity.Server`'s `AuthenticationControllerBase`/`TokenControllerBase` for the server-side handlers.

## Notes

- `ApplicationState.cs` currently only defines `InitialApplicationState { AccessToken, RefreshToken }` — a commented-out `ApplicationState` class remains in the file as a placeholder for future use.
- See the [root README](../README.md#dxblazoridentity-serverwasm) for the full Blazor Server and Blazor Hosted WASM setup walkthrough (DI registration, controllers, and a sample Login.razor).

See the [root README](../README.md) for the full package list and DevExpress version alignment notes.
