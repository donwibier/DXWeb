# DX.Blazor.Identity.Server

Server-side ASP.NET Core Identity integration for Blazor Server apps and the Web API backing a hosted Blazor WASM app — JWT issuing, cookie-based Server sign-in, and the base controllers your project's `AuthenticationController`/`TokenController` derive from.

**Target frameworks:** net9.0, net10.0
**DevExpress dependency:** transitive, via `DX.Blazor.Identity` → `DX.Data.Xpo.Identity` (`26.1.*`)

## Install

```
dotnet add package DX.Blazor.Identity.Server
```

## AuthenticationService<TKey, TUser, TRegistrationModel, TAuthenticationModel> (Services/AuthenticationService.cs)

```cs
public abstract class AuthenticationService<TKey, TUser, TRegistrationModel, TAuthenticationModel> : IAuthService<TRegistrationModel, TAuthenticationModel>
{
    // Uses UserManager<TUser>, IDataProtectionProvider, NavigationManager
    public Task<RegistrationResponseModel> RegisterUser(TRegistrationModel model); // POSTs JSON to /api/Accounts/Registration
    public Task<AuthResponseModel> Login(TAuthenticationModel model);
    public Task Logout();
    public Task<string> RefreshToken();
}

public abstract class AuthenticationService<TKey, TUser, TRegistrationModel> : AuthenticationService<TKey, TUser, TRegistrationModel, AuthenticationModel> { }
public class AuthenticationService<TUser, TRegistrationModel> : AuthenticationService<string, TUser, TRegistrationModel> { }
public class AuthenticationService<TUser> : AuthenticationService<TUser, RegistrationModel>, IAuthService { }
```

This is the Blazor **Server**-specific `IAuthService` implementation. Rather than storing a JWT in browser storage (as the WASM variant does), it protects a token payload with `IDataProtectionProvider` and performs a hard, `NavigationManager.NavigateTo(..., forceLoad: true)` redirect to a `GET /api/Accounts/Login?token=...` endpoint, letting the server sign the user in via cookie authentication (`SignInManager.SignInAsync`) — the right pattern for Blazor Server, where there's no client-side JS to hold a bearer token.

## AuthenticationControllerBase<TKey, TUser, TRegistrationModel> (Controllers/AuthenticationControllerBase.cs)

```cs
public abstract class AuthenticationControllerBase<TKey, TUser, TRegistrationModel> : Controller
{
    [HttpPost("Registration")] public Task<IActionResult> RegisterUser(TRegistrationModel model);
    [HttpPost("Login")] public Task<IActionResult> Login(AuthenticationModel model);   // returns JWT directly (WASM flow)
    [HttpGet("Login")]  public Task<IActionResult> Login(string token);                 // unprotects token, verifies, SignInAsync (Server flow)
    [HttpGet("ExternalLogins")] public IActionResult ExternalLogins();
    [HttpGet("LogOut")] public Task<IActionResult> Logout();
}
```

Your project's own `AuthenticationController` (or `AccountController`) derives from this and supplies `UserManager<TUser>`, `SignInManager<TUser>`, `IDataProtectionProvider`, `ILogger`, and `IConfiguration` via constructor injection — see the [root README](../README.md#dxblazoridentity-serverwasm) for the exact controller shape for both the Blazor Server and Hosted WASM scenarios. The signing key is read from `JwtSettings:securityKey` in `IConfiguration`.

## TokenControllerBase<TKey, TUser> (Controllers/TokenControllerBase.cs)

```cs
public class TokenControllerBase<TKey, TUser> : ControllerBase
{
    [HttpPost("refresh")] public Task<IActionResult> Refresh(RefreshTokenModel model);
}
```

Implements the refresh-token flow via the injected `ITokenService<TKey, TUser>` — used by the hosted WASM scenario's `TokenController`. **Note:** this file physically lives in `DX.Blazor.Identity.Server/Controllers/` but its namespace is `DX.Blazor.Identity.Wasm.Controllers` (a pre-existing inconsistency in the codebase — your `using` statement needs to reference the `Wasm.Controllers` namespace even though the package you installed is `.Server`).

## AuthStateProvider<TUser> (AuthStateStateProvider.cs)

```cs
public class AuthStateProvider<TUser> : RevalidatingServerAuthenticationStateProvider where TUser : class
{
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);
    protected override Task<bool> ValidateAuthenticationStateAsync(AuthenticationState state, CancellationToken ct);
}
```

Registered as `AuthenticationStateProvider` in Blazor Server apps — periodically (every 30 minutes) re-validates the signed-in user's security stamp against a freshly-resolved `UserManager<TUser>` from a new DI scope, so a password change or lockout invalidates the session without waiting for the circuit to reconnect.

## Supporting services (Services/)

```cs
public interface IUserService { string GetCurrentUserId(); string GetCurrentUserName(); }
public class UserService : IUserService { /* reads ClaimTypes.Sid / ClaimTypes.Name off HttpContext.User */ }

public class TokenAccessService { public string GetToken(); /* reads the "access_token" auth-scheme token off HttpContext */ }
```

Small convenience services for pulling the current user's id/name or the raw access token out of `HttpContext` from anywhere in the DI graph (e.g. a scoped service that doesn't have direct controller access).

## TokenProvider (TokenProvider.cs)

```cs
public class TokenProvider { public string AccessToken { get; set; } public string RefreshToken { get; set; } }
```

A trivial scoped DTO used to pass the current token pair around within a single Blazor Server circuit.

## Notes

- See the [root README](../README.md#dxblazoridentity-serverwasm) for the complete DI registration and controller code for both Blazor Server and Blazor Hosted WASM setups.
- Pairs with `DX.Blazor.Identity.Wasm` for the hosted-WASM scenario (this package hosts the Web API; the WASM package is the client).

See the [root README](../README.md) for the full package list and DevExpress version alignment notes.
