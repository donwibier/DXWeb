# DX.Data.EF.AutoMapper

AutoMapper-backed implementation of `DX.Data.EF`'s abstract `EFDataStore<TEFContext, TKey, TModel, TDBModel>`.

**Target frameworks:** net471, net8.0, net9.0, net10.0
**DevExpress dependency:** none

> ⚠ **Requires .NET Framework 4.7.1 or higher.** As of `26.1.3.33` this package no longer supports net462 — see [Breaking Changes in the root README](../README.md#-breaking-changes) (AutoMapper CVE-2026-32933). If you need net462, use `DX.Data.EF.Mapster` instead.

## Install

```
dotnet add package DX.Data.EF.AutoMapper
```

## EFAutoMapperDataStore<TEFContext, TKey, TModel, TDBModel>

```cs
public class EFAutoMapperDataStore<TEFContext, TKey, TModel, TDBModel> : EFDataStore<TEFContext, TKey, TModel, TDBModel>
    where TEFContext : DbContext
    where TDBModel : class
{
    public EFAutoMapperDataStore(TEFContext context, IMapper mapper, IValidator<TDBModel> validator);

    public override IQueryable<T> Query<T>(); // EFQuery().ProjectTo<T>(Mapper.ConfigurationProvider)
    protected override TDBModel ToDBModel(TModel model); // Mapper.Map<TDBModel>(model)
    protected override TModel ToModel(TDBModel dbModel);  // Mapper.Map<TModel>(dbModel)
    protected override TModel GetByKey(TKey key);
}
```

Thin subclass — every mapping-related abstract member from `EFDataStore` delegates straight to the injected `IMapper`. `Query<T>()` uses AutoMapper's `ProjectTo<T>`, so EF Core translates the projection into SQL rather than materializing full entities first.

### Usage

```cs
public class CustomerStore : EFAutoMapperDataStore<AppDbContext, int, CustomerDto, Customer>
{
    public CustomerStore(AppDbContext context, IMapper mapper, IValidator<Customer> validator)
        : base(context, mapper, validator) { }
}
```

Register your `IMapper` (`services.AddAutoMapper(...)` with a `Profile` mapping `CustomerDto <-> Customer`) and an `IValidator<Customer>` (FluentValidation) in DI as usual.

## Notes

- If your project must stay on **net462**, use `DX.Data.EF.Mapster` instead — it has no AutoMapper dependency.
- No DevExpress dependency at all; this package works with any EF Core/EF6 `DbContext`.

See the [root README](../README.md) for the full package list, the AutoMapper breaking-change notice, and DevExpress version alignment notes.
