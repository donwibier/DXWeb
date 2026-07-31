# DX.Data.Xpo.Identity.AutoMapper

Wires up `DX.Data.Xpo.Identity`'s abstract stores with [AutoMapper](https://automapper.org/) and a set of default XPO persistent entities, so you can register full ASP.NET Core Identity support with a single DI call.

**Target frameworks:** net471, net8.0, net9.0, net10.0
**DevExpress dependency:** transitive, via `DX.Data.Xpo` (`26.1.*`)

> ⚠ **Requires .NET Framework 4.7.1 or higher.** As of `26.1.3.34` this package no longer supports net462 — see [Breaking Changes in the root README](../README.md#-breaking-changes) (AutoMapper CVE-2026-32933). If you need net462, use `DX.Data.Xpo.Identity.Mapster` instead.

## Install

```
dotnet add package DX.Data.Xpo.Identity.AutoMapper
```

Install this **or** `DX.Data.Xpo.Identity.Mapster` — never both (see the [root README](../README.md#note)).

## Concrete stores (XPIdentityStores.cs, `#if NETCOREAPP`)

Six concrete classes, one per Identity concern, each `: XPBaseXxxStore<...>` from `DX.Data.Xpo.Identity` implementing `Query<T>()` via AutoMapper's `ProjectTo<T>` and `GetByKey` via `Mapper.Map`:

```cs
public class XPAutoMapperUserStore<...>      : XPBaseUserStore<...>      { public XPAutoMapperUserStore(IDataLayer, IMapper, IValidator<TXPOUser>); }
public class XPAutoMapperRoleStore<...>      : XPBaseRoleStore<...>      { /* ... */ }
public class XPAutoMapperUserLoginStore<...> : XPBaseUserLoginStore<...> { /* ... */ }
public class XPAutoMapperUserClaimStore<...> : XPBaseUserClaimStore<...> { /* ... */ }
public class XPAutoMapperUserTokenStore<...> : XPBaseUserTokenStore<...> { /* ... */ }
public class XPAutoMapperRoleClaimStore<...> : XPBaseRoleClaimStore<...> { /* ... */ }
```

## One-line registration (RegisterServices.cs)

```cs
public static IServiceCollection AddXpoAutoMapperIdentityStores<TUser>(this IdentityBuilder builder, string connectionName);
// + overloads with progressively more generic type parameters if you supply your own
// persistent XPO entity classes instead of the defaults (XpoDxUser, XpoDxRole, etc. from
// DX.Data.Xpo.Identity.Persistent)
```

Calling this:
1. Registers AutoMapper (`services.AddAutoMapper(cfg => cfg.AddProfile(new XPIdentityMapperProfile<...>()))`) if it isn't already registered.
2. Registers a singleton `IDataLayer` resolved from `XpoDatabase.GetDataLayer(connectionName)`.
3. Calls `RegisterXPIdentityValidators<...>()` to register the six FluentValidation validators.
4. Registers all six `IQueryable*Store` services as scoped.
5. Calls `RegisterIdentityServices.AddStores<TKey>(...)`, which builds and registers the actual `IUserStore<TUser>`/`IRoleStore<TRole>` implementations.

`XPIdentityMapperProfile<...>` (also in RegisterServices.cs) is the AutoMapper `Profile` used in step 1 — it maps between the Identity model types and the XPO persistent types in both directions, deliberately ignoring collection-navigation properties (`RolesList`, `ClaimsList`, `TokenList`, `LoginsList`) since those are managed by the store methods, not by a flat object-to-object map.

### Usage

```cs
services
    .AddIdentity<ApplicationUser>(options => { /* lockout, password policy, etc. */ })
    .AddXpoAutoMapperIdentityStores<ApplicationUser>("DefaultConnection")
    .AddDefaultTokenProviders();
```

That's the entire registration — no manual mapper profile, validator, or store wiring required for the default persistent entity classes. See the [root README](../README.md#dxdataxpoidentity) for the full config sample including JWT/token service setup used alongside `DX.Blazor.Identity`.

## Notes

- If you need to use your own XPO persistent classes instead of the built-in `XpoDxUser`/`XpoDxRole`/etc., use one of the more-generic `AddXpoAutoMapperIdentityStores` overloads and supply your own `TXPOUser`/`TXPORole`/... type arguments — AutoMapper will need corresponding map configuration for them (extend or replace `XPIdentityMapperProfile`).
- For Blazor Server/WASM login UI on top of this, see `DX.Blazor.Identity`.

See the [root README](../README.md) for the full package list, the AutoMapper breaking-change notice, and DevExpress version alignment notes.
