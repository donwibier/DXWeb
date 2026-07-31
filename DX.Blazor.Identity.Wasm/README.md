# DX.Blazor.Identity.Wasm

Client-side ASP.NET Core Identity integration for Blazor WebAssembly apps — token storage in `localStorage`, an `AuthenticationStateProvider` that parses JWT claims, and an HTTP interceptor that silently refreshes an about-to-expire token before it's attached to outgoing requests.

**Target frameworks:** net9.0, net10.0
**DevExpress dependency:** transitive, via `DX.Blazor.Identity` → `DX.Data.Xpo.Identity` (`26.1.*`)

## Install

```
dotnet add package DX.Blazor.Identity.Wasm
```

Requires `Blazored.LocalStorage` and `Toolbelt.Blazor.HttpClientInterceptor` (both pulled in automatically as package dependencies).

## AuthenticationService (Services/AuthenticationService.cs)

```cs
public class AuthenticationService<TRegistrationModel, TAuthenticationModel> : IAuthService<TRegistrationModel, TAuthenticationModel>
{
    public AuthenticationService(HttpClient client, AuthenticationStateProvider authStateProvider, ILocalStorageService localStorage);

    public Task<RegistrationResponseModel> RegisterUser(TRegistrationModel model); // POST Accounts/Registration
    public Task<AuthResponseModel> Login(TAuthenticationModel model);              // POST Accounts/Login, stores token+refreshToken in localStorage
    public Task<string> RefreshToken();                                          // POST token/refresh
    public Task Logout();                                                        // clears localStorage, notifies AuthStateProvider
}

public class AuthenticationService<TRegistrationModel> : AuthenticationService<TRegistrationModel, AuthenticationModel>, IAuthService<TRegistrationModel> { }
public class AuthenticationService : AuthenticationService<RegistrationModel>, IAuthService { }
```

On successful login, the JWT and refresh token are written to `localStorage` (keys `authToken`/`refreshToken`), the `HttpClient`'s default `Authorization` header is set to `bearer <token>`, and `AuthStateProvider.NotifyUserAuthentication(token)` is called so the rest of the app immediately sees the new claims principal.

## AuthStateProvider (AuthStateProvider.cs)

```cs
public class AuthStateProvider : AuthenticationStateProvider
{
    public AuthStateProvider(HttpClient httpClient, ILocalStorageService localStorage);
    public override Task<AuthenticationState> GetAuthenticationStateAsync(); // reads authToken from localStorage, parses JWT claims
    public void NotifyUserAuthentication(string token);
    public void NotifyUserLogout();
}

public static class JwtParser
{
    public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt); // base64url-decodes the JWT payload segment into claims
}
```

`JwtParser.ParseClaimsFromJwt` handles the JWT's base64url padding manually (`Convert.FromBase64String` needs `==`/`=` padding restored) and specifically unpacks a JSON array under the `ClaimTypes.Role` key into one `Claim` per role — plain ASP.NET Core `ClaimsPrincipal` parsing doesn't do this for you when a JWT encodes multiple roles as a single array claim.

## TokenService<TKey, TUser> (Services/TokenService.cs)

```cs
public class TokenService<TKey, TUser> : ITokenService<TKey, TUser>
    where TUser : IdentityUser<TKey>, IIdentityRefreshToken, new()
{
    public TokenService(IConfiguration configuration, UserManager<TUser> userManager);
    public SigningCredentials GetSigningCredentials();       // reads JwtSettings:securityKey
    public Task<List<Claim>> GetClaims(TUser user);           // Name + one Role claim per role
    public JwtSecurityToken GenerateTokenOptions(SigningCredentials creds, List<Claim> claims); // JwtSettings:validIssuer/validAudience/expiryInMinutes
    public string GenerateRefreshToken();                     // 32 random bytes, base64
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token); // validates everything except lifetime
}
```

This is the server-side-callable token minting/validation logic — despite living in the `.Wasm` package, it's typically registered and invoked from your **Web API** project (the hosted-WASM backend) alongside `DX.Blazor.Identity.Server`, not executed in the browser itself.

## RefreshTokenService / HttpInterceptorService (Services/)

```cs
public class RefreshTokenService
{
    public RefreshTokenService(AuthenticationStateProvider authProvider, IAuthService authService);
    public Task<string> TryRefreshToken(); // refreshes if the current token's "exp" claim is within 2 minutes of expiring
}

public class HttpInterceptorService
{
    public HttpInterceptorService(HttpClientInterceptor interceptor, RefreshTokenService refreshTokenService);
    public void RegisterEvent();   // hooks HttpClientInterceptor.BeforeSendAsync
    public Task InterceptBeforeHttpAsync(object sender, HttpClientInterceptorEventArgs e);
    public void DisposeEvent();
}
```

`HttpInterceptorService` is the piece that makes silent token refresh actually silent: it hooks into `Toolbelt.Blazor.HttpClientInterceptor`'s `BeforeSendAsync` event, and for every outgoing request whose path doesn't contain `token` or `accounts` (to avoid refreshing while you're literally calling the login/refresh endpoints), it calls `RefreshTokenService.TryRefreshToken()` and — if a new token was minted — overwrites the request's `Authorization` header before it goes out. Register it once at app startup (`RegisterEvent()`) and your Razor components never need to think about token expiry.

## Setup

```cs
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddHttpClientInterceptor();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress + "api/") }.EnableIntercept(sp));
builder.Services.AddScoped<IAuthService<RegistrationModel, AuthenticationModel>, DX.Blazor.Identity.Wasm.Services.AuthenticationService>();
builder.Services.AddScoped<DX.Blazor.Identity.Wasm.Services.RefreshTokenService>();
builder.Services.AddScoped<AuthenticationStateProvider, DX.Blazor.Identity.Wasm.AuthStateProvider>();
builder.Services.AddScoped<DX.Blazor.Identity.Wasm.Services.HttpInterceptorService>();
```

See the [root README](../README.md#dxblazoridentity-serverwasm) for the complete Hosted WASM walkthrough, including the matching server-side controllers from `DX.Blazor.Identity.Server`.

## Notes

- Pairs with `DX.Blazor.Identity.Server`, which hosts the `Accounts/Registration`, `Accounts/Login`, and `token/refresh` API endpoints this package's `HttpClient` calls.
- `SupportedPlatform Include="browser"` is declared in the csproj — this package is intended for the WASM target and depends on `Blazored.LocalStorage`, which requires JS interop.

See the [root README](../README.md) for the full package list and DevExpress version alignment notes.
