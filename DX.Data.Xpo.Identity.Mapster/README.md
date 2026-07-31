# DX.Data.Xpo.Identity.Mapster

Wires up `DX.Data.Xpo.Identity`'s abstract stores with [Mapster](https://github.com/MapsterMapper/Mapster) and a set of default XPO persistent entities — the AutoMapper-free alternative to `DX.Data.Xpo.Identity.AutoMapper`. This is the package to use if your Identity project must stay on **net462**.

**Target frameworks:** net462, net8.0, net9.0, net10.0
**DevExpress dependency:** transitive, via `DX.Data.Xpo` (`26.1.*`)

## Install

```
dotnet add package DX.Data.Xpo.Identity.Mapster
```

Install this **or** `DX.Data.Xpo.Identity.AutoMapper` — never both (see the [root README](../README.md#note)).

## Concrete stores (XPIdentityStores.cs)

Mirrors the AutoMapper package exactly, using Mapster's `IMapper`/`ProjectToType<T>()` instead:

```cs
public class XPMapsterUserStore<...>      : XPBaseUserStore<...>      { public XPMapsterUserStore(IDataLayer, MapsterMapper.IMapper, IValidator<TXPOUser>); }
public class XPMapsterRoleStore<...>      : XPBaseRoleStore<...>      { /* ... */ }
public class XPMapsterUserLoginStore<...> : XPBaseUserLoginStore<...> { /* ... */ }
public class XPMapsterUserClaimStore<...> : XPBaseUserClaimStore<...> { /* ... */ }
public class XPMapsterUserTokenStore<...> : XPBaseUserTokenStore<...> { /* ... */ }
public class XPMapsterRoleClaimStore<...> : XPBaseRoleClaimStore<...> { /* ... */ }
```

## One-line registration (RegisterServices.cs)

```cs
public static IServiceCollection AddXpoMapsterIdentityStores<TUser>(this IdentityBuilder builder, string connectionName);
// + more-generic overloads for supplying your own persistent XPO entity types
```

Unlike the AutoMapper variant, this registers `services.AddTransient<IMapper, Mapper>()` and calls `services.RegisterXPIdentityMapsterConfiguration<...>()` instead of an AutoMapper `Profile`. `XPIdentityMapsterConfig.RegisterXPIdentityMapsterConfiguration<...>` (also shipped in this package) configures `TypeAdapterConfig<TUser, TXPOUser>.NewConfig().Ignore(...)` for the collection-navigation properties, plus `.AfterMapping()` callbacks that restore FK id fields (like `RoleId`/`UserId`) that Mapster's `.Ignore()` would otherwise blank out on the mapped target — a subtlety AutoMapper's profile doesn't need to worry about because of how it handles ignored members differently.

### Usage

```cs
services
    .AddIdentity<ApplicationUser>(options => { /* lockout, password policy, etc. */ })
    .AddXpoMapsterIdentityStores<ApplicationUser>("DefaultConnection")
    .AddDefaultTokenProviders();
```

See the [root README](../README.md#dxdataxpoidentity) for the full config sample including JWT/token service setup used alongside `DX.Blazor.Identity`.

## Notes

- If you're on net462 and were using `DX.Data.Xpo.Identity.AutoMapper` before `26.1.3.34`, this package is your migration path — the registration call and store shapes are deliberately identical (`AddXpoMapsterIdentityStores` vs `AddXpoAutoMapperIdentityStores`), only the underlying mapper changes.
- For Blazor Server/WASM login UI on top of this, see `DX.Blazor.Identity`.

See the [root README](../README.md) for the full package list and DevExpress version alignment notes.
