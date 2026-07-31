# DX.Data.Xpo.Mapster

[Mapster](https://github.com/MapsterMapper/Mapster)-backed implementation of `DX.Data.Xpo`'s abstract `XPDataStore<TKey, TModel, TDBModel>` — the AutoMapper-free alternative to `DX.Data.Xpo.AutoMapper`. This is the package to use if your project needs to keep targeting **net462** (Mapster has no equivalent .NET Framework version-floor issue).

**Target frameworks:** net462, net8.0, net9.0, net10.0
**DevExpress dependency:** transitive, via `DX.Data.Xpo` (`26.1.*`)

## Install

```
dotnet add package DX.Data.Xpo.Mapster
```

## XPMapsterStore<TKey, TModel, TDBModel>

```cs
public class XPMapsterStore<TKey, TModel, TDBModel> : XPDataStore<TKey, TModel, TDBModel>
    where TDBModel : XPBaseObject
{
    public XPMapsterStore(IDataLayer dataLayer, MapsterMapper.IMapper mapper, IValidator<TDBModel> validator);

    public override IQueryable<T> Query<T>(); // Query(UnitOfWork).ProjectToType<T>()
    protected override TDBModel ToDBModel(TModel model); // Mapper.Map<TDBModel>(model)
    protected override TModel ToModel(TDBModel dbModel);  // Mapper.Map<TModel>(dbModel)
    protected override TModel GetByKey(TKey key);
}
```

Mirrors `XPAutoMapperStore` exactly, member for member — swap `AutoMapper.IMapper` for `MapsterMapper.IMapper` and `ProjectTo<T>` for Mapster's `ProjectToType<T>()`. If you're migrating between the two mapper packages, the only code that changes is your `TypeAdapterConfig`/`Profile` setup and DI registration — the store subclass itself stays identical.

### Usage

```cs
public class CustomerStore : XPMapsterStore<int, CustomerDto, XpoCustomer>
{
    public CustomerStore(IDataLayer dataLayer, MapsterMapper.IMapper mapper, IValidator<XpoCustomer> validator)
        : base(dataLayer, mapper, validator) { }
}
```

Configure your `TypeAdapterConfig` (e.g. `TypeAdapterConfig<CustomerDto, XpoCustomer>.NewConfig()...`) and register `MapsterMapper.Mapper` as `IMapper` in DI, then resolve `IDataLayer` the same way as the other XPO store variants.

## Notes

- Package targets `net462` (Mapster 7.2.0), and `net8.0`/`net9.0`/`net10.0` (Mapster 10.x) via conditional `PackageReference`.
- For MS Identity storage on top of XPO with Mapster wiring, see `DX.Data.Xpo.Identity.Mapster`.

See the [root README](../README.md) for the full package list and DevExpress version alignment notes.
