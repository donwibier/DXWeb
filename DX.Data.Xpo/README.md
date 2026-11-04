# DX.Data.Xpo

XPO (DevExpress eXpressPersistent Objects) database configuration and the abstract `IQueryableDataStore` base class for the DTO pattern. This package contains **no mapping implementation** — pick `DX.Data.Xpo.AutoMapper` or `DX.Data.Xpo.Mapster` on top of it (or roll your own `XPDataStore` subclass with manual mapping).

**Target frameworks:** net462, net8.0, net9.0, net10.0
**DevExpress dependency:** `DevExpress.Xpo` `26.1.*`

## Behaviour changes in 26.1.3.41

- `XpoDatabase` no longer sets the process-wide `XpoDefault.DataLayer` implicitly (up to 26.1.3.40 it did, but only with `UpdateSchema = AutoCreateOption.DatabaseAndSchema`, and to whichever database was created last). Opt in with `XpoDatabaseOptions.SetAsXpoDefault = true`.

## Behaviour changes in 26.1.3.40

- `XPDataStore.UpdateAsync` with a default key fails instead of inserting a new row.
- Update/Store results contain every item, in input order (they held only inserted items), with values saved by `OnSaving` copied back. A failed result's `Mode` is the mode of the failing item.
- `DeleteAsync` returns a failed result instead of throwing for non-validation errors.
- On a pooled (SQL) database `DeleteAsync` runs in a database level transaction on a connection of its own (`ExplicitUnitOfWork`). Override `protected virtual Task BeforeDeleteAsync(TDBModel dbModel, Session session)` to do work just before each object is deleted; changes it flushes are rolled back if the delete fails. In-memory and caching data layers keep using a normal `UnitOfWork`.
- Batch Update/Store/Delete load all keys in one query instead of one per item.
- `XpoDatabase.GetDataLayer(name)` honours the name (and throws for an unknown one); first use of a name from several threads creates one data layer; in-memory stores are no longer wrapped in a connection pool; `AddXpoUnitOfWork(connectionName)` uses the named connection.
- `XpoDefault.UseFastAccessors = false` is only applied on .NET Framework.
- `GeoHaversine` criteria are correct for distances over about 6,670 km (they returned about 20,002 km).

## Install

```
dotnet add package DX.Data.Xpo
```

## XpoDatabase — connection & session management (XpoDatabase.cs)

```cs
public class XpoDatabaseOptions
{
    public string Name { get; set; }
    public string ConnectionString { get; set; }
    public bool EnableCaching { get; set; }
    public AutoCreateOption UpdateSchema { get; set; } = AutoCreateOption.None;
    public bool SetAsXpoDefault { get; set; } = false; // also make it XpoDefault.DataLayer
}

public class XpoDatabase
{
    public XpoDatabase(Action<XpoDatabaseOptions> configure);
    public XpoDatabase(params XpoDatabaseOptions[] options); // multi-datalayer support

    public Session GetSession();
    public UnitOfWork GetUnitOfWork();

    public void Execute(Action<Session> action);
    public T Execute<T>(Func<Session, T> action);
    public Task ExecuteAsync(Func<Session, Task> action);
    public Task<T> ExecuteAsync<T>(Func<Session, Task<T>> action);

    public static Session GetSession(string name);
    public static UnitOfWork GetUnitOfWork(string name);
    public static IDataLayer GetDataLayer(XpoDatabaseOptions options);

    public IEnumerable<TDest> CloneCollection<TSource, TDest>(IEnumerable<TSource> source, XpoDatabase destination);
    public TDest Clone<TSource, TDest>(TSource source, XpoDatabase destination);
}
```

`XpoDatabase` wraps DevExpress's `IDataLayer` creation so you don't have to hand-build connection strings and data layers yourself. It supports **multiple named data layers in the same process** — pass an array of `XpoDatabaseOptions` (each with its own `Name`) to register more than one database, then resolve sessions/units of work for a specific one via the static `GetSession(name)`/`GetUnitOfWork(name)` overloads. `CreateDataLayer` (internal) reads `AutoCreateOption` and the `EnableCaching` node straight off the connection string, so schema auto-update and second-level caching are configured the same way DevExpress's own tooling expects.

`XpoDatabase` does not touch `XpoDefault.DataLayer` unless you ask for it: set `SetAsXpoDefault = true` on the options of the database that code creating sessions without a data layer (`new UnitOfWork()`, `new Session()`, an `XPCollection` without a session) should use. It sets `XpoDefault.DataLayer` to that database's own data layer, whatever `UpdateSchema` is. `XpoDefault.Session` is set to `null`, so the default session cannot be picked up by accident.

`Clone`/`CloneCollection` do an object-graph copy of XPO persistent objects from one `XpoDatabase` into another — useful for seeding a test/staging database from production data without a full export/import round-trip.

Two constructor overloads that take a bare `connectionName`/simple parameters are marked `[Obsolete]` — use the `Action<XpoDatabaseOptions>` or `XpoDatabaseOptions[]`-based constructors going forward.

### DI registration (XpoCoreExtensions.cs, `#if NETSTANDARD2_1 || NETCOREAPP`)

```cs
services.AddXpoDatabase(o =>
{
    o.Name = "DefaultConnection";
    o.ConnectionString = Configuration.GetConnectionString("DefaultConnection");
});

// or register several data layers at once:
services.AddXpoDatabases(
    o => { o.Name = "Db1"; o.ConnectionString = "..."; },
    o => { o.Name = "Db2"; o.ConnectionString = "..."; }
);
```

`AddXpoUnitOfWork(connectionName)` and the single-string-argument `AddXpoDatabase(connectionName)` overload are `[Obsolete]` — prefer the options-based registration above.

## XPDataStore<TKey, TModel, TDBModel> — the abstract store base (XPDataStore.cs)

```cs
public abstract class XPDataStore<TKey, TModel, TDBModel> : IQueryableDataStore<TKey, TModel>, IDisposable
    where TDBModel : XPBaseObject
{
    protected XPDataStore(IDataLayer dataLayer, IValidator<TDBModel> validator);

    public abstract IQueryable<T> Query<T>();
    public IQueryable<TModel> Query();

    protected abstract TModel GetByKey(TKey key);
    protected abstract TDBModel ToDBModel(TModel model);
    protected abstract TModel ToModel(TDBModel dbModel);
    protected TDestination MapTo<TSource, TDestination>(TSource source);

    public Task<TResult> TransactionalExecAsync<TResult>(Func<IDataLayer, UnitOfWork, Task<TResult>> action);

    public Task<IDataResult<TKey, TModel>> StoreAsync(params TModel[] models); // Create/Update/Store via FluentValidation
    public Task<IDataResult<TKey, TModel>> DeleteAsync(TKey key);
    public Task<IDataResult<TKey, TModel>> DeleteAsync(TModel model);
}
```

This is the class every concrete XPO-backed store in the DXWeb family derives from — directly (if you write your own mapping in `ToDBModel`/`ToModel`) or through the `XPAutoMapperStore`/`XPMapsterStore` subclasses in the sibling mapper packages, which implement `ToDBModel`/`ToModel`/`Query<T>()` for you via AutoMapper or Mapster. `StoreAsync` runs FluentValidation against the constructor-supplied `IValidator<TDBModel>` before committing, and wraps the DevExpress `UnitOfWork` commit in `TransactionalExecAsync` so failures roll back cleanly. `ThrowIfDisposed`/`ThrowIfNull`/`ThrowIfNullOrEmpty` are protected guard helpers available to derived classes for consistent argument validation.

### Minimal usage (writing your own mapping, no AutoMapper/Mapster)

```cs
public class CustomerStore : XPDataStore<int, CustomerDto, XpoCustomer>
{
    public CustomerStore(IDataLayer dataLayer, IValidator<XpoCustomer> validator)
        : base(dataLayer, validator) { }

    public override IQueryable<T> Query<T>() => /* project XpoCustomer -> T yourself */;
    protected override CustomerDto GetByKey(int key) => /* ... */;
    protected override XpoCustomer ToDBModel(CustomerDto model) => /* ... */;
    protected override CustomerDto ToModel(XpoCustomer dbModel) => /* ... */;
}
```

In practice, most consumers skip writing `ToDBModel`/`ToModel`/`Query<T>()` by hand and instead derive from `XPAutoMapperStore`/`XPMapsterStore` (see `DX.Data.Xpo.AutoMapper` / `DX.Data.Xpo.Mapster`), which implement those members via a configured `IMapper`.

## Legacy types (kept for source compatibility)

- `IXPDataMapper<TKey, TModel, TXPOClass>` (XpoInterfaces.cs) — **`[Obsolete]`**
- `XPDataMapper<TKey, TModel, TXPOClass>` (XPDataMapper.cs) — **`[Obsolete]`**, superseded by the AutoMapper/Mapster store variants.

## Notes

- This package has a genuine transitive DevExpress dependency (`DevExpress.Xpo`) and is version-locked to the DXWeb release wave (`26.1.3.x` ↔ DevExpress v26.1.3).
- If you need MS Identity storage on top of XPO, see `DX.Data.Xpo.Identity` (plus its `.AutoMapper`/`.Mapster` variant).
- If you're building an ASP.NET MVC 5 `ASPxGridView`, see `DX.Data.Xpo.Mvc`.

See the [root README](../README.md) for the full package list and DevExpress version alignment notes.
