using System;
using System.Linq;
using AutoMapper;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;
using DX.Data.Xpo.AutoMapper;
using DX.Data.Xpo.Mapster;
using FluentValidation;
using Mapster;
using Microsoft.Extensions.Logging.Abstractions;

namespace DX.Data.Xpo.Tests
{
    // ---------- persistent classes ----------

    [Persistent("TestProduct")]
    public class Product : XPObject
    {
        public Product(Session session) : base(session) { }

        private string _name;
        [Size(100)]
        public string Name { get => _name; set => SetPropertyValue(nameof(Name), ref _name, value); }

        private decimal _price;
        public decimal Price { get => _price; set => SetPropertyValue(nameof(Price), ref _price, value); }

        private double _lat;
        public double Lat { get => _lat; set => SetPropertyValue(nameof(Lat), ref _lat, value); }

        private double _lon;
        public double Lon { get => _lon; set => SetPropertyValue(nameof(Lon), ref _lon, value); }

        private string _stamp;
        /// <summary>Set by OnSaving, so the tests can see whether server side changes come back into the model</summary>
        public string Stamp { get => _stamp; set => SetPropertyValue(nameof(Stamp), ref _stamp, value); }

        protected override void OnSaving()
        {
            base.OnSaving();
            Stamp = "saved:" + Name;
        }
    }

    /// <summary>A persistent class without [Persistent], as most XPO classes are written</summary>
    public class PlainNote : XPObject
    {
        public PlainNote(Session session) : base(session) { }

        private string _text;
        public string Text { get => _text; set => SetPropertyValue(nameof(Text), ref _text, value); }
    }

    [Persistent("TestUniqueCode")]
    public class UniqueCode : XPObject
    {
        public UniqueCode(Session session) : base(session) { }

        private string _code;
        [Indexed(Unique = true), Size(50)]
        public string Code { get => _code; set => SetPropertyValue(nameof(Code), ref _code, value); }
    }

    // ---------- DTO ----------

    public class ProductModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public string Stamp { get; set; }
    }

    public class ProductNameModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class UniqueCodeModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
    }

    // ---------- validators ----------

    public class ProductValidator : AbstractValidator<Product>
    {
        public int Calls;
        public DataMode? LastMode;
        public object LastStore;

        public ProductValidator()
        {
            RuleFor(p => p.Name).NotEmpty().WithMessage("Name is required");
            RuleFor(p => p.Price).GreaterThanOrEqualTo(0).WithMessage("Price must not be negative");
            RuleFor(p => p).Custom((p, ctx) =>
            {
                System.Threading.Interlocked.Increment(ref Calls);
                LastMode = (DataMode)ctx.RootContextData[XPDataStore<int, ProductModel, Product>.CtxMode];
                LastStore = ctx.RootContextData[XPDataStore<int, ProductModel, Product>.CtxStore];
                if (LastMode == DataMode.Delete && p.Name == "Undeletable")
                    ctx.AddFailure("Name", "This product cannot be deleted");
            });
        }
    }

    public class UniqueCodeValidator : AbstractValidator<UniqueCode> { }

    // ---------- stores ----------

    public class ProductMapsterStore : XPMapsterStore<int, ProductModel, Product>
    {
        public ProductMapsterStore(IDataLayer dataLayer, MapsterMapper.IMapper mapper, IValidator<Product> validator)
            : base(dataLayer, mapper, validator) { }

        public override string KeyField => nameof(ProductModel.Id);
        public override int ModelKey(ProductModel model) => model.Id;
        public override void SetModelKey(ProductModel model, int key) => model.Id = key;
        protected override int DBModelKey(Product model) => model.Oid;

        public System.Threading.Tasks.Task<System.Collections.Generic.List<ProductModel>> PublicFindAsync(DevExpress.Data.Filtering.CriteriaOperator filter)
            => FindAsync(filter);

        public UnitOfWork CurrentUnitOfWork => UnitOfWork;
    }

    public class ProductAutoMapperStore : XPAutoMapperStore<int, ProductModel, Product>
    {
        public ProductAutoMapperStore(IDataLayer dataLayer, global::AutoMapper.IMapper mapper, IValidator<Product> validator)
            : base(dataLayer, mapper, validator) { }

        public override string KeyField => nameof(ProductModel.Id);
        public override int ModelKey(ProductModel model) => model.Id;
        public override void SetModelKey(ProductModel model, int key) => model.Id = key;
        protected override int DBModelKey(Product model) => model.Oid;
    }

    public class UniqueCodeStore : XPMapsterStore<int, UniqueCodeModel, UniqueCode>
    {
        public UniqueCodeStore(IDataLayer dataLayer, MapsterMapper.IMapper mapper, IValidator<UniqueCode> validator)
            : base(dataLayer, mapper, validator) { }

        public override string KeyField => nameof(UniqueCodeModel.Id);
        public override int ModelKey(UniqueCodeModel model) => model.Id;
        public override void SetModelKey(UniqueCodeModel model, int key) => model.Id = key;
        protected override int DBModelKey(UniqueCode model) => model.Oid;
    }

    // ---------- fixtures ----------

    public static class Mappers
    {
        public static MapsterMapper.IMapper Mapster()
        {
            var cfg = new TypeAdapterConfig();
            cfg.NewConfig<Product, ProductModel>().Map(d => d.Id, s => s.Oid);
            cfg.NewConfig<Product, ProductNameModel>().Map(d => d.Id, s => s.Oid);
            cfg.NewConfig<ProductModel, Product>()
                .Ignore(d => d.Oid).Ignore(d => d.Stamp)
                .IgnoreNonMapped(true)
                .Map(d => d.Name, s => s.Name).Map(d => d.Price, s => s.Price)
                .Map(d => d.Lat, s => s.Lat).Map(d => d.Lon, s => s.Lon);
            cfg.NewConfig<UniqueCode, UniqueCodeModel>().Map(d => d.Id, s => s.Oid);
            cfg.NewConfig<UniqueCodeModel, UniqueCode>().IgnoreNonMapped(true).Map(d => d.Code, s => s.Code);
            return new MapsterMapper.Mapper(cfg);
        }

        public static global::AutoMapper.IMapper AutoMapper()
        {
            var cfg = new MapperConfiguration(c =>
            {
                c.CreateMap<Product, ProductModel>().ForMember(d => d.Id, o => o.MapFrom(s => s.Oid));
                c.CreateMap<Product, ProductNameModel>().ForMember(d => d.Id, o => o.MapFrom(s => s.Oid));
                c.CreateMap<ProductModel, Product>(MemberList.None)
                    .ForMember(d => d.Stamp, o => o.Ignore());
            }, NullLoggerFactory.Instance);
            return cfg.CreateMapper();
        }
    }

    /// <summary>A fresh in-memory XPO database per test, with schema for the test classes</summary>
    public sealed class InMemoryDb
    {
        public InMemoryDb()
        {
            XpoDefault.Session = null;
            var dictionary = new ReflectionDictionary();
            dictionary.GetDataStoreSchema(typeof(Product), typeof(PlainNote), typeof(UniqueCode));
            Store = new InMemoryDataStore(AutoCreateOption.DatabaseAndSchema);
            DataLayer = new ThreadSafeDataLayer(dictionary, Store);
        }

        public InMemoryDataStore Store { get; }
        public IDataLayer DataLayer { get; }

        public UnitOfWork NewUnitOfWork() => new UnitOfWork(DataLayer);

        public int Count<T>() where T : class
        {
            using var uow = NewUnitOfWork();
            return uow.Query<T>().Count();
        }

        public int AddProduct(string name, decimal price = 1m, double lat = 0, double lon = 0)
        {
            using var uow = NewUnitOfWork();
            var p = new Product(uow) { Name = name, Price = price, Lat = lat, Lon = lon };
            uow.CommitChanges();
            return p.Oid;
        }

        public Product Load(UnitOfWork uow, int id) => uow.GetObjectByKey<Product>(id);
    }
}
