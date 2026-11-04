using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;
using FluentValidation;
using Xunit;

namespace DX.Data.Xpo.Tests
{
    /// <summary>Counts the SELECT round-trips that reach the underlying data store</summary>
    public sealed class CountingDataStore : IDataStore
    {
        private readonly IDataStore _inner;
        private int _selects;

        public CountingDataStore(IDataStore inner) => _inner = inner;

        public int Selects => Volatile.Read(ref _selects);
        public void Reset() => Interlocked.Exchange(ref _selects, 0);

        public AutoCreateOption AutoCreateOption => _inner.AutoCreateOption;
        public ModificationResult ModifyData(params ModificationStatement[] dmlStatements) => _inner.ModifyData(dmlStatements);
        public SelectedData SelectData(params SelectStatement[] selects)
        {
            Interlocked.Increment(ref _selects);
            return _inner.SelectData(selects);
        }
        public UpdateSchemaResult UpdateSchema(bool doNotCreateIfFirstTableNotExist, params DBTable[] tables)
            => _inner.UpdateSchema(doNotCreateIfFirstTableNotExist, tables);
    }

    /// <summary>
    /// Regression tests for the batched (prefetching) Update/Store/Delete in XPDataStore:
    /// round-trip count, result order and semantics must match the one-by-one implementation
    /// </summary>
    public abstract class BatchTestsBase
    {
        protected readonly InMemoryDb Db = new InMemoryDb();
        protected readonly CountingDataStore Counter;
        protected readonly IDataLayer CountingLayer;

        protected BatchTestsBase()
        {
            Counter = new CountingDataStore(Db.Store);
            var dictionary = new ReflectionDictionary();
            dictionary.GetDataStoreSchema(typeof(Product), typeof(PlainNote), typeof(UniqueCode));
            CountingLayer = new ThreadSafeDataLayer(dictionary, Counter);
        }

        protected abstract XPDataStore<int, ProductModel, Product> CreateStore(IDataLayer dataLayer, IValidator<Product> validator);

        private int[] Seed(int count) => Enumerable.Range(1, count).Select(i => Db.AddProduct("P" + i, i)).ToArray();

        /// <summary>XPO loads its XPObjectType records once per data layer; do that before counting</summary>
        protected void WarmUp()
        {
            using var uow = new UnitOfWork(CountingLayer);
            new Product(uow) { Name = "warm" };
            uow.CommitChanges();
            uow.Delete(uow.Query<Product>().Single(p => p.Name == "warm"));
            uow.PurgeDeletedObjects();
            uow.CommitChanges();
        }

        [Fact]
        public async Task UpdateBatch_LoadsExistingObjectsInOneRoundTrip()
        {
            var ids = Seed(20);
            using var store = CreateStore(CountingLayer, new ProductValidator());
            var models = ids.Select(id => new ProductModel { Id = id, Name = "U" + id, Price = 1 }).ToArray();

            WarmUp();
            Counter.Reset();
            var result = await store.UpdateAsync(models);

            Assert.True(result.Success, result.Exception?.Message);
            Assert.Equal(1, Counter.Selects);
            Assert.Equal(models, result.Data);
            using var uow = Db.NewUnitOfWork();
            foreach (var id in ids)
                Assert.Equal("U" + id, Db.Load(uow, id).Name);
        }

        [Fact]
        public async Task DeleteBatch_LoadsExistingObjectsInOneRoundTrip()
        {
            var ids = Seed(20);
            using var store = CreateStore(CountingLayer, new ProductValidator());

            WarmUp();
            Counter.Reset();
            var result = await store.DeleteAsync(ids.Take(15).ToArray());

            Assert.True(result.Success, result.Exception?.Message);
            Assert.Equal(1, Counter.Selects);
            Assert.Equal(5, Db.Count<Product>());
        }

        [Fact]
        public async Task StoreBatch_Mixed_KeepsInputOrderAndKeys()
        {
            var ids = Seed(3);
            using var store = CreateStore(CountingLayer, new ProductValidator());
            var models = new[]
            {
                new ProductModel { Name = "new1" },
                new ProductModel { Id = ids[2], Name = "upd3" },
                new ProductModel { Name = "new2" },
                new ProductModel { Id = ids[0], Name = "upd1" },
            };

            var result = await store.StoreAsync(models);

            Assert.True(result.Success, result.Exception?.Message);
            Assert.Equal(models, result.Data);
            Assert.Equal(ids[2], models[1].Id);
            Assert.Equal(ids[0], models[3].Id);
            Assert.DoesNotContain(models[0].Id, ids.Append(0));
            Assert.DoesNotContain(models[2].Id, ids.Append(0));
            Assert.All(models, m => Assert.Equal("saved:" + m.Name, m.Stamp));
            Assert.Equal(5, Db.Count<Product>());
        }

        [Fact]
        public async Task UpdateBatch_SameKeyTwice_LastWinsAndBothModelsMappedBack()
        {
            var id = Db.AddProduct("Old");
            using var store = CreateStore(CountingLayer, new ProductValidator());
            var first = new ProductModel { Id = id, Name = "First" };
            var second = new ProductModel { Id = id, Name = "Second" };

            var result = await store.UpdateAsync(first, second);

            Assert.True(result.Success, result.Exception?.Message);
            Assert.Equal(new[] { first, second }, result.Data);
            using var uow = Db.NewUnitOfWork();
            Assert.Equal("Second", Db.Load(uow, id).Name);
            Assert.Equal("saved:Second", first.Stamp);
            Assert.Equal("saved:Second", second.Stamp);
        }

        [Fact]
        public async Task UpdateBatch_UnknownKeyInMiddle_FailsOnThatItemAndRollsBack()
        {
            var ids = Seed(3);
            using var store = CreateStore(CountingLayer, new ProductValidator());
            var ghost = new ProductModel { Id = 9999, Name = "Ghost" };
            var result = await store.UpdateAsync(
                new ProductModel { Id = ids[0], Name = "A" }, ghost, new ProductModel { Id = ids[1], Name = "B" });

            Assert.False(result.Success);
            Assert.Equal(DataMode.Update, result.Mode);
            Assert.Same(ghost, Assert.Single(result.Data));
            Assert.Contains("Unable to locate", result.Exception.Message);
            using var uow = Db.NewUnitOfWork();
            Assert.Equal("P1", Db.Load(uow, ids[0]).Name);
        }

        [Fact]
        public async Task DeleteBatch_WithDuplicatesAndUnknownKeys_Succeeds()
        {
            var ids = Seed(3);
            using var store = CreateStore(CountingLayer, new ProductValidator());
            var result = await store.DeleteAsync(ids[0], 9999, ids[0], 0, ids[1]);
            Assert.True(result.Success, result.Exception?.Message);
            Assert.Equal(1, Db.Count<Product>());
        }
    }

    public class MapsterBatchTests : BatchTestsBase
    {
        protected override XPDataStore<int, ProductModel, Product> CreateStore(IDataLayer dataLayer, IValidator<Product> validator)
            => new ProductMapsterStore(dataLayer, Mappers.Mapster(), validator);

        /// <summary>
        /// A store whose sessions use XPO's default weak identity map (XpoDatabase switches the process to a strong one,
        /// so whether a test sees it depends on test order)
        /// </summary>
        private sealed class WeakIdentityMapStore : ProductMapsterStore
        {
            public WeakIdentityMapStore(IDataLayer dataLayer, IValidator<Product> validator) : base(dataLayer, Mappers.Mapster(), validator) { }

            protected override System.Collections.ICollection XPOPrefetchByKeys(System.Collections.Generic.IEnumerable<int> keys, Session session)
            {
                session.IdentityMapBehavior = IdentityMapBehavior.Weak;
                return base.XPOPrefetchByKeys(keys, session);
            }
        }

        /// <summary>Collects garbage while each item is validated, i.e. between the prefetch and the next item's lookup</summary>
        private sealed class CollectingValidator : AbstractValidator<Product>
        {
            public CollectingValidator() => RuleFor(p => p).Custom((p, ctx) => { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); });
        }

        [Fact]
        public async Task UpdateBatch_WithAWeakIdentityMap_KeepsThePrefetchedObjects()
        {
            var ids = Enumerable.Range(1, 20).Select(i => Db.AddProduct("P" + i, i)).ToArray();
            using var store = new WeakIdentityMapStore(CountingLayer, new CollectingValidator());
            var models = ids.Select(id => new ProductModel { Id = id, Name = "U" + id, Price = 1 }).ToArray();
            WarmUp();
            Counter.Reset();

            var result = await store.UpdateAsync(models);

            Assert.True(result.Success, result.Exception?.Message);
            Assert.Equal(1, Counter.Selects);
        }

        [Fact]
        public async Task DeleteBatch_WithAWeakIdentityMap_KeepsThePrefetchedObjects()
        {
            var ids = Enumerable.Range(1, 20).Select(i => Db.AddProduct("P" + i, i)).ToArray();
            using var store = new WeakIdentityMapStore(CountingLayer, new CollectingValidator());
            WarmUp();
            Counter.Reset();

            var result = await store.DeleteAsync(ids);

            Assert.True(result.Success, result.Exception?.Message);
            Assert.Equal(1, Counter.Selects);
        }

        [Fact]
        public async Task OverriddenXPOGetByKey_IsStillUsedForEveryItem()
        {
            var ids = Enumerable.Range(1, 4).Select(i => Db.AddProduct("P" + i)).ToArray();
            using var store = new LookupCountingStore(CountingLayer, Mappers.Mapster(), new ProductValidator());
            var result = await store.UpdateAsync(ids.Select(id => new ProductModel { Id = id, Name = "X" + id }).ToArray());
            Assert.True(result.Success, result.Exception?.Message);
            Assert.Equal(4, store.Lookups);
            Assert.True((await store.DeleteAsync(ids)).Success);
            Assert.Equal(8, store.Lookups);
            Assert.Equal(0, Db.Count<Product>());
        }

        private sealed class LookupCountingStore : ProductMapsterStore
        {
            public LookupCountingStore(IDataLayer dataLayer, MapsterMapper.IMapper mapper, IValidator<Product> validator)
                : base(dataLayer, mapper, validator) { }

            public int Lookups;

            protected override Product XPOGetByKey(int key, Session session)
            {
                Lookups++;
                return base.XPOGetByKey(key, session);
            }
        }
    }

    public class AutoMapperBatchTests : BatchTestsBase
    {
        protected override XPDataStore<int, ProductModel, Product> CreateStore(IDataLayer dataLayer, IValidator<Product> validator)
            => new ProductAutoMapperStore(dataLayer, Mappers.AutoMapper(), validator);
    }
}
