# DXWeb 
Repository for several DevExpress releated NuGet packages:
These packages follow the minimum requirements as described in DevExpress documentation.

**Minimum framework versions: .NET Framework 4.6.2, .NET 8.0 and .NET 9.0** — except the AutoMapper-based packages, which require **.NET Framework 4.7.1** minimum (see Breaking Changes below).

**Current DevExpress version: v26.1.3** (packages reference `26.1.*`, so you automatically pick up the latest 26.1.x patch on restore)

## ⚠ Breaking Changes

### DX.Data.Xpo.AutoMapper, DX.Data.Xpo.Identity.AutoMapper, DX.Data.EF.AutoMapper — .NET Framework 4.6.2 support dropped

As of version `26.1.3.34` (`26.1.3.33` for DX.Data.EF.AutoMapper), these three packages **no longer support net462**. The minimum .NET Framework target is now **4.7.1 (net471)**. The .NET 8/9/10 targets are unaffected.

**Why:** AutoMapper 10.1.1 (the version these packages used on net462) is affected by [CVE-2026-32933 / GHSA-rvv3-g6hj-g44x](https://github.com/advisories/GHSA-rvv3-g6hj-g44x) — a high-severity Denial-of-Service vulnerability where mapping a deeply nested or circular object graph triggers an unrecoverable `StackOverflowException` and crashes the process. Every AutoMapper release below 15.1.3/16.1.1 is affected, including 10.1.1, and it will not be patched — AutoMapper no longer publishes any version that supports .NET Framework below 4.7.1. There is no version of AutoMapper that is both patched and net462-compatible, so the only way to get the fix is to move to net471 and AutoMapper 16.x.

**What this means for you:**

- If your project already targets net471 or higher (net472, net48, net481, or any .NET 8+ TFM), just take the update — nothing else changes.
- If your project targets net462, net463, net47, or net470, restoring the new package version will fail with a NU1202 "package not compatible" error at build time (not a silent runtime break). You have two options:
  - Stay on the previous package version and accept the AutoMapper vulnerability (mitigate in code by calling `.MaxDepth(64)` — or similar — on every `CreateMap` call to cap recursion), or
  - Switch to the Mapster-based equivalent (`DX.Data.Xpo.Mapster`, `DX.Data.Xpo.Identity.Mapster`, `DX.Data.EF.Mapster`), which still supports net462 and doesn't depend on AutoMapper at all.

## Installation

All packages in this repo are published to [NuGet.org](https://www.nuget.org). Install the ones you need with the .NET CLI or the NuGet Package Manager, e.g.:

```
dotnet add package DX.Data.Xpo
dotnet add package DX.Data.Xpo.Identity
dotnet add package DX.Data.Xpo.Identity.AutoMapper   # or DX.Data.Xpo.Identity.Mapster, not both
```

Since DevExpress v25.1, DevExpress packages (`DevExpress.Xpo`, `DevExpress.Data`, `DevExpress.Web.Mvc5`, `DevExpress.Blazor`, etc.) are published directly on nuget.org as well, so restoring any package below pulls in the matching DevExpress dependency automatically — **no separate DevExpress private NuGet feed is required just to build**. You do still need a valid, active [DevExpress subscription/license](https://www.devexpress.com/products/) registered on your machine (or a license key at build/runtime, depending on the product) to legally use the underlying DevExpress components.

| Package | Purpose |
|---|---|
| `DX.Utils` | Helper classes (attributes, bit operations, collections, conversion, logging, URLs) |
| `DX.Data` | Base `DataStore<TKey, TModel>` abstraction, DTO mapping + FluentValidation, Blazor WASM `ApiDataStore` |
| `DX.Data.Xpo` | `XpoDatabase`/`XpoDataStore` for DTO pattern, no mapper included |
| `DX.Data.Xpo.AutoMapper` | Same as above, using AutoMapper for DTO mapping — **requires net471+** (see Breaking Changes) |
| `DX.Data.Xpo.Mapster` | Same as above, using Mapster for DTO mapping |
| `DX.Data.Xpo.Interfaces` | Shared interfaces for `DX.Data.Xpo.Identity` |
| `DX.Data.Xpo.Identity` | Abstract XPO-based storage for ASP.NET/Microsoft Identity |
| `DX.Data.Xpo.Identity.AutoMapper` | `DX.Data.Xpo.Identity` wired up with AutoMapper — **requires net471+** (see Breaking Changes) |
| `DX.Data.Xpo.Identity.Mapster` | `DX.Data.Xpo.Identity` wired up with Mapster |
| `DX.Data.EF` | Base EF Core `IDataStore` implementation, no mapper included |
| `DX.Data.EF.AutoMapper` | Same as above, using AutoMapper — **requires net471+** (see Breaking Changes) |
| `DX.Data.EF.Mapster` | Same as above, using Mapster |
| `DX.Blazor.Identity` / `.Server` / `.Wasm` | Ready-made MS Identity integration for Blazor Server & WASM |
| `DX.Data.Xpo.Mvc` | ASPxGridView (ASP.NET MVC 5) extensions on top of `DX.Data.Xpo` — requires an active DevExpress ASP.NET license |

#### DX.Utils
This package contains several helper classes for working with:
* Attributes
* Bit operations
* Collections
* Conversion helpers
* Logging helpers
* Url manipulations

#### DX.Data
* DataStore<TKey, TModel> base class to work with DataSets ->
  Abstract DTO Mapping, FluentValidator validation and ApiDataStore for BlazorWASM with CRUD support.

#### DX.Data.Xpo
This package contains the XpoDatabase and XpoDataStore for easy config and use of DTO pattern **no mapping implementation**

#### DX.Data.Xpo.AutoMapper
This package contains the XpoDatabase and XpoDataStore for easy config and use of DTO pattern by using AutoMapper.

**Requires .NET Framework 4.7.1 or higher (net471+); net462 is no longer supported as of `26.1.3.34`** — see [Breaking Changes](#-breaking-changes). If you need net462, use `DX.Data.Xpo.Mapster` instead.

#### DX.Data.Xpo.Mapster
This package contains the XpoDatabase and XpoDataStore for easy config and use of DTO pattern by using Mapster

#### DX.Data.Xpo.Interfaces
Shared interfaces used by **DX.Data.Xpo.Identity** to support ASP.NET/Microsoft Identity storage. Referenced automatically as a dependency — you normally don't need to install it directly.

#### DX.Data.Xpo.Identity
This package contains an XPO based *abstract* storage mechanism for use with MS Identity to support a small dozen different DB engines.

#### DX.Data.Xpo.Identity.AutoMapper
This package contains an XPO based storage mechanism for use with MS Identity to support a small dozen different DB engines with AutMapper DTO handling.

**Requires .NET Framework 4.7.1 or higher (net471+); net462 is no longer supported as of `26.1.3.34`** — see [Breaking Changes](#-breaking-changes). If you need net462, use `DX.Data.Xpo.Identity.Mapster` instead.

#### DX.Data.Xpo.Identity.Mapster
This package contains an XPO based storage mechanism for use with MS Identity to support a small dozen different DB engines with Mapster DTO handling.

#### Note
From v23.2.3.31, you will need to include either **DX.Data.Xpo.Identity.AutoMapper**  or **DX.Data.Xpo.Identity.Mapster** (**NOT BOTH**), depending on the DTO Mapper you're already (or planning) using! Note that as of `26.1.3.34`, the AutoMapper variant requires net471+ — if your project is still on net462, **DX.Data.Xpo.Identity.Mapster** is your only option.

#### DX.Data.EF
Abstract EF Core based `DX.Data.IDataStore` implementation with FluentValidation, **no mapping implementation** included — pick one of the mapper-specific packages below (or roll your own) on top of it.

#### DX.Data.EF.AutoMapper
`DX.Data.IDataStore` implementation for use with EF Core, using FluentValidation and AutoMapper for DTO mapping.

**Requires .NET Framework 4.7.1 or higher (net471+); net462 is no longer supported as of `26.1.3.33`** — see [Breaking Changes](#-breaking-changes). If you need net462, use `DX.Data.EF.Mapster` instead.

#### DX.Data.EF.Mapster
`DX.Data.IDataStore` implementation for use with EF Core, using FluentValidation and Mapster for DTO mapping.

As with the XPO Identity packages, install either the AutoMapper or the Mapster variant depending on which mapper you're standardized on — not both. If you're on net462, Mapster is your only option.


Quick config on .NET:
```cs
public void ConfigureServices(IServiceCollection services)
        {
            string connStrName = "DefaultConnection";
            //Initialize XPODataLayer / Database	
            services.AddXpoDatabase((o) => {
                o.Name = connStrName;
                o.ConnectionString = Configuration.GetConnectionString(connStrName);
            });

            //Initialize identity to use XPO
            services
                .AddIdentity<ApplicationUser>(options => {
                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    options.Lockout.MaxFailedAccessAttempts = 3;
                })
                /***** OLD OBSOLETE CONFIG < 23.2.3.31 
                .AddXpoIdentityStores<ApplicationUser, XpoApplicationUser>(connStrName,
					new ApplicationUserMapper(), 
					new ApplicationRoleMapper(),
					new XPUserStoreValidator<string, ApplicationUser, XpoApplicationUser>(),
					new XPRoleStoreValidator<string, ApplicationRole, XpoApplicationRole>())
		*/
		// When using AutoMapper
                .AddXpoAutoMapperIdentityStores<ApplicationUser/*, XpoApplicationUser*/>(connStrName)
		// When using Mapster
                .AddXpoMapsterIdentityStores<ApplicationUser/*, XpoApplicationUser*/>(connStrName)
                .AddDefaultTokenProviders();

		// token config
		builder.Services.AddScoped<ITokenService<string, ApplicationUser>, TokenService<string, ApplicationUser> >();

		var jwtSettings = builder.Configuration.GetSection("JWTSettings");
		builder.Services.AddAuthentication(opt =>
		{
			opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		}).AddJwtBearer(options =>
		{
			options.TokenValidationParameters = new TokenValidationParameters
			{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,

			ValidIssuer = jwtSettings["validIssuer"],
			ValidAudience = jwtSettings["validAudience"],
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["securityKey"]))
			};
		});


            // Add other services ...
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddControllersWithViews();
            services.AddRazorPages();
        }

```

#### DX.Blazor.Identity (Server/Wasm)

Initial publish with code to make incoporate MS Identity in you Blazor apps simple

##### Blazor Server steps:
Add package DX.Blazor.Identity.Server to project

In Startup.ConfigureServices:
```cs
			services.AddScoped<IAuthService<RegisterUserModel>, AuthenticationService<ApplicationUser, RegisterUserModel>>();
			services.AddScoped<DX.Blazor.Identity.Server.TokenProvider>();
			services.AddScoped<AuthenticationStateProvider, DX.Blazor.Identity.Server.AuthStateProvider<ApplicationUser>>();
```
Create AuthorizationController:

```cs
    [Route("api/accounts")]
    public class AuthenticationController : AuthenticationControllerBase<ApplicationUser>
    {
        public AuthenticationController(UserManager<ApplicationUser> userManager, 
                SignInManager<ApplicationUser> signInManager, 
                IDataProtectionProvider dataProtectionProvider, 
                ILogger<AuthenticationControllerBase<string, ApplicationUser, RegisterUserModel>> logger, IConfiguration configuration) 
                : base(userManager, signInManager, dataProtectionProvider, logger, configuration)
        {

        }

    }

```


##### Blazor Hosted WASM steps:
In web api add DX.Blazor.Identity.Server

In startup.ConfigureServices (or Program.cs)

```c
builder.Services.AddScoped<IAuthService<RegistrationModel, AuthenticationModel>, AuthenticationService<ApplicationUser, RegistrationModel>>();
builder.Services.AddScoped<DX.Blazor.Identity.Server.TokenProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, DX.Blazor.Identity.Server.AuthStateProvider<ApplicationUser>>();
```

Create AuthorizationController and TokenController:

```cs

    [Route("api/accounts")]
    public class AccountController : AuthenticationControllerBase<ApplicationUser>
    {
        public AccountController(UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, 
            IDataProtectionProvider dataProtectionProvider, 
            ILogger<AuthenticationControllerBase<ApplicationUser>> logger, 
            IConfiguration configuration) 
            : base(userManager, signInManager, dataProtectionProvider, logger, configuration)
        {

        }
    }


    [Route("api/token")]
	[ApiController]
	public class TokenController : TokenControllerBase<string, ApplicationUser>
    {
        public TokenController(UserManager<ApplicationUser> userManager, 
			ITokenService<string, ApplicationUser> tokenService) : base(userManager, tokenService)
        {

        }
    }

```


In WASM project add DX.Blazor.Identity.WASM, Blazored.LocalStorage and Toolbelt.HttpClientInterceptor

In Startup.ConfigureServices (or Program.cs)

```cs
    builder.Services.AddBlazoredLocalStorage(); //Blazored.LocalStorage
    builder.Services.AddHttpClientInterceptor(); //Toolbelt.HttpClientInterceptor
    //builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
    builder.Services.AddScoped(sp => new HttpClient
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress + "api/")
    }
    .EnableIntercept(sp));
    builder.Services.AddScoped<IAuthService<RegistrationModel, AuthenticationModel>, DX.Blazor.Identity.Wasm.Services.AuthenticationService>();
    builder.Services.AddScoped<DX.Blazor.Identity.Wasm.Services.RefreshTokenService>();
    builder.Services.AddScoped<AuthenticationStateProvider, DX.Blazor.Identity.Wasm.AuthStateProvider>();
    builder.Services.AddScoped<DX.Blazor.Identity.Wasm.Services.HttpInterceptorService>();

```


For both projects (WASM and Server) you can use the following Login.razor:


```cs
@inject NavigationManager navigationManager
@inject DX.Blazor.Identity.IAuthService AuthenticationService 

<EditForm Model="@userModel" .... >

</EditForm>

@code {
    AuthenticationModel usermodel = new AuthenticationModel();

    // ....

    protected async Task LoginAction()
    {
        errors.Clear();
        userModel.ReturnUrl = redirectUrl;
        var result = await AuthenticationService.Login(userModel);
        if (!result.IsAuthSuccessful)
        {
            errors.Add(result.ErrorMessage);
        }
        else
        {
            navigationManager.NavigateTo("/");
        }
    }
}



#### DX.Data.Xpo.Mvc (For ASP.NET Framework ASPxGridView MVC Extension only)

**Please note:** _You will need at least an **active** [DevExpress ASP.NET **License**](https://www.devexpress.com/products/net/controls/asp/) for this package_

Some helper classes to have the ASPxGridView extensions support server-side filtering and sorting based on the XPDataStore implementation.
