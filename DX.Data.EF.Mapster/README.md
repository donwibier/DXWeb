# DX.Data.EF.Mapster

[Mapster](https://github.com/MapsterMapper/Mapster)-backed implementation of `DX.Data.EF`'s abstract `EFDataStore<TEFContext, TKey, TModel, TDBModel>` — the AutoMapper-free alternative to `DX.Data.EF.AutoMapper`. Use this if your project needs to keep targeting **net462**.

**Target frameworks:** net462, net8.0, net9.0, net10.0
**DevExpress dependency:** none

## Install

```
dotnet add package DX.Data.EF.Mapster
```

## EFMapsterDataStore<TEFContext, TKey, TModel, TDBModel>

```cs
public class EFMapsterDataStore<TEFContext, TKey, TModel, TDBModel> : EFDataStore<TEFContext, TKey, TModel, TDBModel>
    where TEFContext : DbContext
    where TDBModel : class
{
    public EFMapsterDataStore(TEFContext context, MapsterMapper.IMapper mapper, IValidator<TDBModel> validator);

    public override IQueryable<T> Query<T>(); // EFQuery().ProjectToType<T>()
    protected override TDBModel ToDBModel(TModel model); // Mapper.Map<TDBModel>(model)
    protected override TModel ToModel(TDBModel dbModel);  // Mapper.Map<TModel>(dbModel)
    protected override TModel GetByKey(TKey key);
}
```

Mirrors `EFAutoMapperDataStore` exactly — swap `AutoMapper.IMapper` for `MapsterMapper.IMapper` and `ProjectTo<T>` for Mapster's `ProjectToType<T>()`.

### Usage

```cs
public class CustomerStore : EFMapsterDataStore<AppDbContext, int, CustomerDto, Customer>
{
    public CustomerStore(AppDbContext context, MapsterMapper.IMapper mapper, IValidator<Customer> validator)
        : base(context, mapper, validator) { }
}
```

Configure your `TypeAdapterConfig` and register `MapsterMapper.Mapper` as `IMapper` in DI, same as the XPO Mapster variant.

## Notes

- net462 uses Mapster 7.2.0 (plus `EntityFramework` 6.4.4 for EF6); net8.0/9.0/10.0 use Mapster 10.x and the matching `Microsoft.EntityFrameworkCore` major version.
- No DevExpress dependency.

See the [root README](../README.md) for the full package list and DevExpress version alignment notes.
