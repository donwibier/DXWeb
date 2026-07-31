# DX.Data.Xpo.AutoMapper

AutoMapper-backed implementation of `DX.Data.Xpo`'s abstract `XPDataStore<TKey, TModel, TDBModel>` — DTO ↔ XPO persistent-object mapping handled by [AutoMapper](https://automapper.org/) instead of hand-written code.

**Target frameworks:** net471, net8.0, net9.0, net10.0
**DevExpress dependency:** transitive, via `DX.Data.Xpo` (`26.1.*`)

> ⚠ **Requires .NET Framework 4.7.1 or higher.** As of `26.1.3.34` this package no longer supports net462 — see [Breaking Changes in the root README](../README.md#-breaking-changes) for why (AutoMapper CVE-2026-32933) and what to do if you can't move off net462 (use `DX.Data.Xpo.Mapster` instead).

## Install

```
dotnet add package DX.Data.Xpo.AutoMapper
```

## XPAutoMapperStore<TKey, TModel, TDBModel>

```cs
public class XPAutoMapperStore<TKey, TModel, TDBModel> : XPDataStore<TKey, TModel, TDBModel>
    where TDBModel : XPBaseObject
{
    public XPAutoMapperStore(IDataLayer dataLayer, IMapper mapper, IValidator<TDBModel> validator);

    public override IQueryable<T> Query<T>(); // Query(UnitOfWork).ProjectTo<T>(Mapper.ConfigurationProvider)
    protected override TDBModel ToDBModel(TModel model); // Mapper.Map<TDBModel>(model)
    protected override TModel ToModel(TDBModel dbModel);  // Mapper.Map<TModel>(dbModel)
    protected override TModel GetByKey(TKey key);          // Mapper.Map<TModel>(session.GetObjectByKey<TDBModel>(key))
}
```

This is a thin, fully-generic subclass of `XPDataStore` — all the mapping methods it overrides just delegate to the injected `IMapper`. `Query<T>()` uses AutoMapper's `ProjectTo<T>` (server-side projection, translated into the underlying SQL/criteria by AutoMapper's `IConfigurationProvider`), so filtering/paging happens in the database rather than in memory.

### Usage

```cs
public class CustomerStore : XPAutoMapperStore<int, CustomerDto, XpoCustomer>
{
    public CustomerStore(IDataLayer dataLayer, IMapper mapper, IValidator<XpoCustomer> validator)
        : base(dataLayer, mapper, validator) { }
}
```

Register the `IMapper` the normal AutoMapper way (`services.AddAutoMapper(...)` with a `Profile` that maps `CustomerDto <-> XpoCustomer`), inject an `IValidator<XpoCustomer>` (FluentValidation), and resolve `IDataLayer` from `XpoDatabase.GetDataLayer(...)` or your DI container.

## Notes

- If your project must stay on **net462**, this package is not an option as of `26.1.3.34` — switch to `DX.Data.Xpo.Mapster`, which has no AutoMapper dependency and continues to support net462.
- For MS Identity storage on top of XPO with AutoMapper wiring, see `DX.Data.Xpo.Identity.AutoMapper`.

See the [root README](../README.md) for the full package list, the AutoMapper breaking-change notice, and DevExpress version alignment notes.
