# DX.Data

The base data-access abstraction shared by every store implementation in the DXWeb family (XPO, EF Core, Blazor WASM). Defines the `IDataStore<TKey, TModel>` contract that `DX.Data.Xpo`, `DX.Data.EF`, and their AutoMapper/Mapster variants all implement, plus a ready-to-use HTTP-based store for Blazor WASM clients. No DevExpress dependency.

**Target frameworks:** net462, net8.0, net9.0, net10.0

## Install

```
dotnet add package DX.Data
```

You normally don't install this directly — it comes in transitively via `DX.Data.Xpo`, `DX.Data.EF`, or their mapper variants. Install it directly only if you're writing your own `IDataStore` implementation from scratch (e.g. against a different backend).

## Core interfaces (Interfaces.cs)

```cs
public enum DataMode { Create, Update, Delete, Store }

public interface IDataStore<TKey, TModel>
{
    Task<IDataResult<TKey, TModel>> DeleteAsync(TKey key);
    string KeyField { get; }
    TKey GetByKey(TKey key); // returns the model, not just the key — see remarks
    TKey ModelKey(TModel model);
    void SetModelKey(TModel model, TKey key);
    Task<IDataResult<TKey, TModel>> StoreAsync(TModel model);
    Task<IDataResult<TKey, TModel>> CreateAsync(TModel model);
    Task<IDataResult<TKey, TModel>> UpdateAsync(TModel model);
}

public interface IQueryableDataStore<TKey, TModel> : IDataStore<TKey, TModel>
{
    IQueryable<TModel> PaginateViaPrimaryKey(IQueryable<TModel> query, int skip, int take);
    IQueryable<T> Query<T>();
    IQueryable<TModel> Query();
}
```

Every concrete store in the DXWeb family (`XPDataStore<...>` in `DX.Data.Xpo`, `EFDataStore<...>` in `DX.Data.EF`) implements `IQueryableDataStore<TKey, TModel>`. `StoreAsync` is the "upsert" entry point: pass a model with a default/empty key to insert, or an existing key to update — `CreateAsync`/`UpdateAsync` are explicit variants of the same underlying logic.

`IDataResult<TKey, TModel>` wraps the outcome of any of the above calls:

```cs
public interface IDataResult<TKey, TModel>
{
    bool Success { get; }
    DataMode Mode { get; }
    TModel? Model { get; }
    Exception? Exception { get; }
}
```

`DataResult<TKey, TModel>` is the concrete implementation, constructed as `new DataResult<TKey, TModel>(mode, propertyName, exception)` — check `.Success` before trusting `.Model`.

Other supporting interfaces:
- `IIdentityRefreshToken` — `RefreshToken` / `RefreshTokenExpiryTime` properties, implemented by identity user models across `DX.Data.Xpo.Identity` and `DX.Blazor.Identity` so refresh-token flows work uniformly.
- `IAssignable` — a marker for types that know how to copy their own state from a source object (`Assign(object source)`), used by the reflection-based `PropertyExtensions.Assign` in `DX.Utils`.
- `IDataMapper<TKey, TModel, TDBModel>` — **`[Obsolete]`**. Superseded by AutoMapper/Mapster-based mapping; kept only so old code referencing it still compiles.

## ApiStore — Blazor WASM REST client (`#if NET5_0_OR_GREATER`)

```cs
public class ApiStore<TKey, TModel> : IDataStore<TKey, TModel>
{
    public ApiStore(HttpClient client, string route);
    // implements CreateAsync/UpdateAsync/StoreAsync/DeleteAsync via HTTP POST/PUT/DELETE
}
```

A drop-in `IDataStore` implementation for Blazor WASM (or any client-side .NET) that talks to a `ControllerBase`-style REST endpoint instead of a database directly. `Create`/`Update`/`Store` POST/PUT the model as JSON; `Delete` issues an HTTP DELETE by key. On a non-success response it deserializes the response body into a `ValidationException` so client code sees the same validation errors the server-side FluentValidation pipeline raised. Pair this with a matching ASP.NET Core controller that wraps a server-side `XPDataStore`/`EFDataStore` to get a full client↔server CRUD round-trip with one consistent validation error shape.

## PredicateBuilder — composable LINQ expressions

```cs
public static class PredicateBuilder
{
    public static Expression<Func<T, bool>> True<T>();
    public static Expression<Func<T, bool>> False<T>();
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2);
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2);
}
```

Lets you build up a filter predicate incrementally (e.g. from a set of optional search filters) without hand-rolling expression-tree combination:

```cs
var predicate = PredicateBuilder.True<Customer>();
if (!string.IsNullOrEmpty(name))
    predicate = predicate.And(c => c.Name.Contains(name));
if (activeOnly)
    predicate = predicate.And(c => c.IsActive);

var results = store.Query().Where(predicate).ToList();
```

## Notes

- This package has no direct DevExpress dependency — it's the shared contract layer that both the XPO-backed and EF-Core-backed stores implement, so switching your data-access technology later doesn't require rewriting consumer code that only depends on `IDataStore<TKey, TModel>`.
- `[Obsolete] DataMapper<TKey, TModel, TDBModel>` (DataMapper.cs) is kept for source compatibility only — use the AutoMapper- or Mapster-backed store variants instead.

See the [root README](../README.md) for the full package list and DevExpress version alignment notes.
