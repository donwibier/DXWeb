# DX.Data.EF

Abstract `DX.Data.IDataStore` implementation backed by Entity Framework (EF Core on net8.0/net9.0/net10.0, classic `System.Data.Entity` on net462). This package contains **no mapping implementation** — pick `DX.Data.EF.AutoMapper` or `DX.Data.EF.Mapster` on top of it, or subclass `EFDataStore` directly and write your own mapping.

**Target frameworks:** net462, net8.0, net9.0, net10.0
**DevExpress dependency:** none — this package works with a plain EF Core `DbContext` and has nothing to do with XPO.

## Install

```
dotnet add package DX.Data.EF
```

## EFDatabase<TEFContext> — transaction helper (EFDatabase.cs)

```cs
public class EFDatabase<TEFContext> where TEFContext : DbContext, new()
{
    public void Execute(Action<TEFContext> action);
    public T Execute<T>(Func<TEFContext, T> action);
    public Task ExecuteAsync(Func<TEFContext, Task> action);
    public Task<T> ExecuteAsync<T>(Func<TEFContext, Task<T>> action);
}
```

Wraps a `DbContext` instance plus an explicit transaction (`IDbContextTransaction` on `NET6_0_OR_GREATER`, classic `DbContextTransaction` on net462) so callers get automatic commit/rollback semantics without repeating `BeginTransaction`/`Commit`/`Rollback` boilerplate at every call site.

## EFDataStore<TEFContext, TKey, TModel, TDBModel> — the abstract store base (EFDataStore.cs)

```cs
public abstract class EFDataStore<TEFContext, TKey, TModel, TDBModel> : IQueryableDataStore<TKey, TModel>
    where TEFContext : DbContext
    where TDBModel : class
{
    protected EFDataStore(TEFContext context, IValidator<TDBModel> validator);

    public abstract IQueryable<T> Query<T>();
    protected IQueryable<TDBModel> EFQuery();
    protected TModel EFGetByKey(TKey key);

    protected abstract TModel GetByKey(TKey key);
    protected abstract TDBModel ToDBModel(TModel model);
    protected abstract TModel ToModel(TDBModel dbModel);
    protected abstract TKey ModelKey(TModel model);
    protected abstract void SetModelKey(TModel model, TKey key);
    protected abstract TKey DBModelKey(TDBModel dbModel);

    public Task<TResult> TransactionalExecAsync<TResult>(Func<TEFContext, Task<TResult>> action);

    public Task<IDataResult<TKey, TModel>> StoreAsync(TModel model); // Add or Entry(...).State = Modified
    public Task<IDataResult<TKey, TModel>> DeleteAsync(TKey key);
    public Task<IDataResult<TKey, TModel>> DeleteAsync(TModel model);
}
```

This is the EF-Core mirror of `DX.Data.Xpo`'s `XPDataStore` — same shape, same `IQueryableDataStore<TKey, TModel>` contract, so consumer code written against the abstraction doesn't care whether the underlying store is XPO- or EF-backed. `StoreAsync` decides between `DbContext.Set<TDBModel>().Add(...)` and `Entry(...).State = EntityState.Modified` based on whether the model's key is set, then commits inside `TransactionalExecAsync`.

### Minimal usage (writing your own mapping, no AutoMapper/Mapster)

```cs
public class CustomerStore : EFDataStore<AppDbContext, int, CustomerDto, Customer>
{
    public CustomerStore(AppDbContext context, IValidator<Customer> validator) : base(context, validator) { }

    public override IQueryable<T> Query<T>() => /* project Customer -> T yourself */;
    protected override CustomerDto GetByKey(int key) => /* ... */;
    protected override Customer ToDBModel(CustomerDto model) => /* ... */;
    protected override CustomerDto ToModel(Customer dbModel) => /* ... */;
    protected override int ModelKey(CustomerDto model) => model.Id;
    protected override void SetModelKey(CustomerDto model, int key) => model.Id = key;
    protected override int DBModelKey(Customer dbModel) => dbModel.Id;
}
```

In practice, most consumers skip this and derive from `EFAutoMapperDataStore`/`EFMapsterDataStore` instead (see the sibling packages), which implement the mapping members via a configured `IMapper`.

## Notes

- No direct DevExpress dependency — bumped alongside the DXWeb release wave purely for repo-wide version consistency, not because of any DevExpress upgrade.
- Depends on `DX.Data` (for `IQueryableDataStore`/`IDataResult`) and `DX.Utils`.
- net462 uses `EntityFramework` 6.4.4 (classic EF6); net8.0/9.0/10.0 use the matching `Microsoft.EntityFrameworkCore` major version.

See the [root README](../README.md) for the full package list and DevExpress version alignment notes.
