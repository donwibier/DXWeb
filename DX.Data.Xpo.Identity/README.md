# DX.Data.Xpo.Identity

Abstract XPO-based storage for ASP.NET Core Identity (`Microsoft.AspNetCore.Identity`) / legacy ASP.NET Identity, supporting the full dozen-plus database engines XPO itself supports (SQL Server, PostgreSQL, MySQL, SQLite, Oracle, DB2, Firebird, and more). This package contains **no mapper wiring** — install `DX.Data.Xpo.Identity.AutoMapper` or `DX.Data.Xpo.Identity.Mapster` (not both) on top of it to get a ready-to-register set of `IUserStore`/`IRoleStore` implementations.

**Target frameworks:** net462, net8.0, net9.0, net10.0
**DevExpress dependency:** `DevExpress.Xpo` `26.1.*`

## Install

```
dotnet add package DX.Data.Xpo.Identity
```

You'll almost always want `DX.Data.Xpo.Identity.AutoMapper` or `DX.Data.Xpo.Identity.Mapster` instead of (well, on top of) this package directly — see the [root README](../README.md#note) for the "pick one, not both" note.

## Interfaces (XPInterfaces.cs)

The persistent-object contracts your XPO entity classes must implement:

```cs
public interface IXPUser<TKey>
{
    string NormalizedUserName { get; set; }
    string NormalizedEmail { get; set; }
    string Email { get; set; }
    string PasswordHash { get; set; }
    string SecurityStamp { get; set; }
    string PhoneNumber { get; set; }
    bool TwoFactorEnabled { get; set; }
    bool LockoutEnabled { get; set; }
    string RefreshToken { get; set; }         // ties into DX.Data.IIdentityRefreshToken
    DateTime? RefreshTokenExpiryTime { get; set; }
    IList RolesList { get; }
    IList ClaimsList { get; }
    IList LoginsList { get; }
    IList TokenList { get; }
    // ...
}

public interface IXPRole<TKey> { /* Name, NormalizedName, ... */ }
public interface IXPUserRole<TKey> { /* ... */ }
public interface IXPUserLogin<TKey> { void InitializeUserLogin(...); /* ... */ }
public interface IXPBaseClaim<TKey> { Claim ToClaim(); void InitializeFromClaim(Claim claim); }
public interface IXPUserClaim<TKey> : IXPBaseClaim<TKey> { void InitializeUserClaim(...); }
public interface IXPRoleClaim<TKey> : IXPBaseClaim<TKey> { void InitializeRoleClaim(...); }
public interface IXPUserToken<TKey> { /* Name, Value, LoginProvider ... */ }
```

Then the "queryable store" interfaces (each extending `IQueryableDataStore<TKey, TModel>` from `DX.Data`) that the mapper packages implement concretely:

```cs
public interface IQueryableUserStore<TKey, TUser, TUserRole, TUserToken> : IQueryableDataStore<TKey, TUser> { /* FindByUserNameAsync, AddToRoleAsync, GetRolesAsync, ... */ }
public interface IQueryableRoleStore<TKey, TRole> : IQueryableDataStore<TKey, TRole> { /* FindByNameAsync, ... */ }
public interface IQueryableUserClaimStore<TKey, TUserClaim> : IQueryableDataStore<TKey, TUserClaim> { /* GetUserClaimsAsync, ... */ }
public interface IQueryableRoleClaimStore<TKey, TRoleClaim> : IQueryableDataStore<TKey, TRoleClaim> { /* GetRoleClaimsAsync, ... */ }
public interface IQueryableUserLoginStore<TKey, TUserLogin> : IQueryableDataStore<TKey, TUserLogin> { /* FindUserLoginAsync, ... */ }
public interface IQueryableUserTokenStore<TKey, TUserToken> : IQueryableDataStore<TKey, TUserToken> { /* FindTokenAsync, ... */ }
```

## Abstract store base classes (XPIdentityDataStores.cs)

Six abstract classes, each `XPDataStore<...>` + the matching `IQueryable*Store` interface, implementing every Identity operation (`AddClaimsAsync`, `AddLoginAsync`, `AddToRoleAsync`, `GetRolesAsync`, `IsInRoleAsync`, `RemoveClaimsAsync`, `ReplaceClaimAsync`, `SetPasswordHashAsync`, `GetSecurityStampAsync`, etc.) against XPO's `XPCollection`/`CriteriaOperator`:

- `XPBaseUserStore<TKey, TUser, TRole, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim, TXPOUser, TXPORole, TXPOLogin, TXPOClaim, TXPOToken>`
- `XPBaseRoleStore<...>`
- `XPBaseUserLoginStore<...>`
- `XPBaseUserClaimStore<...>`
- `XPBaseUserTokenStore<...>`
- `XPBaseRoleClaimStore<...>`

These are the classes `DX.Data.Xpo.Identity.AutoMapper`/`.Mapster` derive from (`XPAutoMapperUserStore`, `XPMapsterUserStore`, etc.), plugging in the mapper-specific `ToModel`/`ToDBModel`/`Query<T>` implementations.

## Concrete ASP.NET Core Identity adapters (XPUserStore.cs, XPRoleStore.cs, `#if NETCOREAPP`)

```cs
public class XPUserStore<TKey, TUser, TRole, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim,
                          TXPOUser, TXPORole, TXPOLogin, TXPOClaim, TXPOToken>
    : UserStoreBase<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>
{
    public XPUserStore(IQueryableUserStore<...> userStore, IQueryableRoleStore<...> roleStore,
                        IQueryableUserClaimStore<...> claimStore, IQueryableUserLoginStore<...> loginStore,
                        IQueryableUserTokenStore<...> tokenStore, IdentityErrorDescriber? describer = null);
}

public class XPRoleStore<TKey, TRole, TUserRole, TRoleClaim, TXPORole, TXPOClaim>
    : RoleStoreBase<TRole, TKey, TUserRole, TRoleClaim>
{
    public XPRoleStore(IQueryableRoleStore<TKey, TRole> roleStore, IQueryableRoleClaimStore<TKey, TRoleClaim> roleClaimStore,
                        IdentityErrorDescriber? describer = null);
}
```

These are the actual `Microsoft.AspNetCore.Identity.UserStoreBase`/`RoleStoreBase` implementations that get registered as `IUserStore<TUser>`/`IRoleStore<TRole>` — they compose the six `IQueryable*Store` services above rather than talking to XPO directly. You don't normally construct these yourself; `RegisterServices.AddStores<TKey>(...)` builds and registers them for you via reflection based on the concrete types you supply.

A non-.NET-Core legacy `XPUserStore<...>` (targeting `Microsoft.AspNet.Identity`'s classic `IUserStore<TUser, TKey>` + friends) also exists in the same file for old-style ASP.NET Identity 2.x consumers.

## RegisterServices.cs — DI wiring

```cs
public static class RegisterIdentityServices
{
    public static void RegisterXPIdentityValidators<...>(this IServiceCollection services); // registers 6 FluentValidation validators
    public static IServiceCollection AddStores<TKey>(this IServiceCollection services, /* concrete user/role/etc. type params */);
}
```

`AddStores<TKey>` is the reflection-heavy method that builds closed generic `XPUserStore<...>`/`XPRoleStore<...>` types via `MakeGenericType` and registers them as `IUserStore<>`/`IRoleStore<>` with `TryAddScoped` — resolving `XpoDatabase` either from an already-registered instance or by building one scoped to a specific connection name. You typically don't call this directly either; `DX.Data.Xpo.Identity.AutoMapper`'s `AddXpoAutoMapperIdentityStores(...)` (or the Mapster equivalent) calls it for you as the last step of a one-line registration.

## Persistent entity classes (XpoDiagramCode/)

Default, ready-to-use XPO persistent classes implementing the interfaces above: `XpoDxUser`, `XpoDxRole`, `XpoDxUserClaim`, `XpoDxUserLogin`, `XpoDxUserToken`, `XpoDxRoleClaim`, and the shared `XpoDxBase`/`XpoDxBaseClaim`. These live in the `DX.Data.Xpo.Identity.Persistent` namespace and are what `AddXpoAutoMapperIdentityStores`/`AddXpoMapsterIdentityStores` default to if you don't supply your own persistent types. `XpoDxUser` overrides `OnDeleting` to detach the user from all roles and delete its claims before the user record itself is removed (referential-integrity cleanup XPO doesn't do for you automatically on a many-to-many).

## Notes

- Since `23.2.3.31` you install exactly one of `DX.Data.Xpo.Identity.AutoMapper` / `DX.Data.Xpo.Identity.Mapster` alongside this package — never both.
- `Bits.cs` defines `DxIdentityRoleFlags` (`FLAG_USERS`, `FLAG_FULL`), a legacy loading-flags scheme from an older, non-reflection-based registration path that's mostly superseded now.
- `XPUserMapper<...>`/`XPRoleMapper<...>` and `XPRoleStoreValidator<...>` are **`[Obsolete]`**/commented-out legacy types — use the `AutoMapper`/`Mapster` packages' mapping instead.

See the [root README](../README.md) for the quick-start DI configuration sample and full package list.
