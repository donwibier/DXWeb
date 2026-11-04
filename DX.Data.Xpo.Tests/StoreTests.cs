using System;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using FluentValidation;
using Xunit;

namespace DX.Data.Xpo.Tests
{
    /// <summary>
    /// CRUD through an XPDataStore descendant. Runs once for the Mapster store and once for the AutoMapper store.
    /// </summary>
    public abstract class StoreTestsBase : IDisposable
    {
        protected readonly InMemoryDb Db = new InMemoryDb();
        protected readonly ProductValidator Validator = new ProductValidator();
        protected readonly XPDataStore<int, ProductModel, Product> Store;

        protected StoreTestsBase()
        {
            Store = CreateStore(Db.DataLayer, Validator);
        }

        protected abstract XPDataStore<int, ProductModel, Product> CreateStore(IDataLayer dataLayer, IValidator<Product> validator);

        public void Dispose() => Store.Dispose();

        private static ProductModel New(string name, decimal price = 1m) => new ProductModel { Name = name, Price = price };

        // ---------- Create ----------

        [Fact]
        public async Task Create_PersistsAndSetsGeneratedKey()
        {
            var model = New("Apple", 2.5m);
            var result = await Store.CreateAsync(model);

            Assert.True(result.Success, result.Exception?.Message);
            Assert.Equal(DataMode.Create, result.Mode);
            Assert.NotEqual(0, model.Id);
            Assert.Same(model, Assert.Single(result.Data));
            using var uow = Db.NewUnitOfWork();
            var db = Db.Load(uow, model.Id);
            Assert.Equal("Apple", db.Name);
            Assert.Equal(2.5m, db.Price);
        }

        [Fact]
        public async Task Create_MapsServerSideChangesBackIntoModel()
        {
            var model = New("Pear");
            var result = await Store.CreateAsync(model);
            Assert.True(result.Success);
            // Product.OnSaving sets Stamp; ObjectSaved maps the db object back into the model
            Assert.Equal("saved:Pear", model.Stamp);
        }

        [Fact]
        public async Task Create_Batch_AssignsDistinctKeysInOrder()
        {
            var items = Enumerable.Range(1, 5).Select(i => New("P" + i)).ToArray();
            var result = await Store.CreateAsync(items);

            Assert.True(result.Success);
            Assert.Equal(5, result.Data.Length);
            Assert.Equal(5, items.Select(i => i.Id).Distinct().Count());
            Assert.DoesNotContain(0, items.Select(i => i.Id));
            Assert.Equal(5, Db.Count<Product>());
            using var uow = Db.NewUnitOfWork();
            foreach (var i in items)
                Assert.Equal(i.Name, Db.Load(uow, i.Id).Name);
        }

        [Fact]
        public async Task Create_WithExistingKey_StillCreatesNewRecord()
        {
            var id = Db.AddProduct("Existing");
            var model = new ProductModel { Id = id, Name = "Copy" };
            var result = await Store.CreateAsync(model);
            Assert.True(result.Success);
            Assert.NotEqual(id, model.Id);
            Assert.Equal(2, Db.Count<Product>());
        }

        [Fact]
        public async Task Create_Invalid_ReturnsValidationErrorsAndPersistsNothing()
        {
            var result = await Store.CreateAsync(New(""));

            Assert.False(result.Success);
            Assert.Equal(DataMode.Create, result.Mode);
            Assert.Contains(result.Exception.Errors, e => e.ErrorMessage == "Name is required");
            Assert.Equal(0, Db.Count<Product>());
        }

        [Fact]
        public async Task Create_BatchWithOneInvalid_RollsBackAll()
        {
            var result = await Store.CreateAsync(New("Good1"), New("Bad", -1), New("Good2"));

            Assert.False(result.Success);
            Assert.Contains(result.Exception.Errors, e => e.ErrorMessage == "Price must not be negative");
            Assert.Equal("Bad", Assert.Single(result.Data).Name);
            Assert.Equal(0, Db.Count<Product>());
        }

        [Fact]
        public async Task Create_NullArray_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => Store.CreateAsync((ProductModel[])null));
        }

        [Fact]
        public async Task Create_EmptyArray_Succeeds()
        {
            var result = await Store.CreateAsync();
            Assert.True(result.Success);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task Create_NullItem_ReturnsFailure()
        {
            var result = await Store.CreateAsync(new ProductModel[] { null });
            Assert.False(result.Success);
            Assert.NotNull(result.Exception);
        }

        [Fact]
        public async Task Create_PassesModeAndStoreToValidator()
        {
            await Store.CreateAsync(New("Ctx"));
            Assert.Equal(DataMode.Create, Validator.LastMode);
            Assert.Same(Store, Validator.LastStore);
        }

        [Fact]
        public async Task Create_CommitFailure_ReturnsFailedResult()
        {
            using var store = new UniqueCodeStore(Db.DataLayer, Mappers.Mapster(), new UniqueCodeValidator());
            Assert.True((await store.CreateAsync(new UniqueCodeModel { Code = "A" })).Success);

            var result = await store.CreateAsync(new UniqueCodeModel { Code = "A" });

            Assert.False(result.Success);
            Assert.NotNull(result.Exception);
            Assert.Equal(1, Db.Count<UniqueCode>());
        }

        // ---------- Update ----------

        [Fact]
        public async Task Update_ChangesExistingRecord()
        {
            var id = Db.AddProduct("Old", 1m);
            var result = await Store.UpdateAsync(new ProductModel { Id = id, Name = "New", Price = 3m });

            Assert.True(result.Success, result.Exception?.Message);
            Assert.Equal(DataMode.Update, result.Mode);
            Assert.Equal(1, Db.Count<Product>());
            using var uow = Db.NewUnitOfWork();
            Assert.Equal("New", Db.Load(uow, id).Name);
            Assert.Equal(3m, Db.Load(uow, id).Price);
        }

        [Fact]
        public async Task Update_ReturnsUpdatedItemsInData()
        {
            var id = Db.AddProduct("Old");
            var model = new ProductModel { Id = id, Name = "New" };
            var result = await Store.UpdateAsync(model);

            Assert.True(result.Success);
            Assert.Same(model, Assert.Single(result.Data));
        }

        [Fact]
        public async Task Update_MapsServerSideChangesBackIntoModel()
        {
            var id = Db.AddProduct("Old");
            var model = new ProductModel { Id = id, Name = "New" };
            await Store.UpdateAsync(model);
            Assert.Equal("saved:New", model.Stamp);
        }

        [Fact]
        public async Task Update_Invalid_ReportsUpdateMode()
        {
            var id = Db.AddProduct("Old");
            var result = await Store.UpdateAsync(new ProductModel { Id = id, Name = "" });

            Assert.False(result.Success);
            Assert.Equal(DataMode.Update, result.Mode);
        }

        [Fact]
        public async Task Update_Invalid_LeavesRecordUnchanged()
        {
            var id = Db.AddProduct("Old");
            var result = await Store.UpdateAsync(new ProductModel { Id = id, Name = "" });

            Assert.False(result.Success);
            Assert.Contains(result.Exception.Errors, e => e.ErrorMessage == "Name is required");
            using var uow = Db.NewUnitOfWork();
            Assert.Equal("Old", Db.Load(uow, id).Name);
        }

        [Fact]
        public async Task Update_UnknownKey_ReturnsFailure()
        {
            var result = await Store.UpdateAsync(new ProductModel { Id = 12345, Name = "Ghost" });

            Assert.False(result.Success);
            Assert.Contains("Unable to locate", result.Exception.Message);
            Assert.Equal(0, Db.Count<Product>());
        }

        [Fact]
        public async Task Update_WithDefaultKey_DoesNotInsert()
        {
            var result = await Store.UpdateAsync(new ProductModel { Id = 0, Name = "NotThere" });

            Assert.False(result.Success);
            Assert.Equal(0, Db.Count<Product>());
        }

        [Fact]
        public async Task Update_PassesUpdateModeToValidator()
        {
            var id = Db.AddProduct("Old");
            await Store.UpdateAsync(new ProductModel { Id = id, Name = "New" });
            Assert.Equal(DataMode.Update, Validator.LastMode);
        }

        // ---------- Store (upsert) ----------

        [Fact]
        public async Task Store_InsertsNewAndUpdatesExisting()
        {
            var id = Db.AddProduct("Existing");
            var existing = new ProductModel { Id = id, Name = "Existing2" };
            var added = New("Added");

            var result = await Store.StoreAsync(existing, added);

            Assert.True(result.Success);
            Assert.Equal(2, Db.Count<Product>());
            Assert.NotEqual(0, added.Id);
            using var uow = Db.NewUnitOfWork();
            Assert.Equal("Existing2", Db.Load(uow, id).Name);
        }

        [Fact]
        public async Task Store_ReturnsAllItemsInData()
        {
            var id = Db.AddProduct("Existing");
            var existing = new ProductModel { Id = id, Name = "Existing2" };
            var added = New("Added");

            var result = await Store.StoreAsync(existing, added);

            Assert.Equal(2, result.Data.Length);
            Assert.Contains(existing, result.Data);
            Assert.Contains(added, result.Data);
        }

        // ---------- Delete ----------

        [Fact]
        public async Task DeleteByKey_RemovesRecord()
        {
            var id = Db.AddProduct("ToDelete");
            var keep = Db.AddProduct("Keep");
            var result = await Store.DeleteAsync(id);

            Assert.True(result.Success);
            Assert.Equal(DataMode.Delete, result.Mode);
            Assert.Equal(1, Db.Count<Product>());
            using var uow = Db.NewUnitOfWork();
            // XPObject uses deferred deletion: the row stays, marked deleted (GCRecord)
            var deleted = Db.Load(uow, id);
            Assert.True(deleted == null || deleted.IsDeleted);
            Assert.False(Db.Load(uow, keep).IsDeleted);
            Assert.Equal(DataMode.Delete, Validator.LastMode);
        }

        [Fact]
        public async Task DeleteByModel_RemovesRecord()
        {
            var model = New("ToDelete");
            await Store.CreateAsync(model);
            var result = await Store.DeleteAsync(model);
            Assert.True(result.Success);
            Assert.Equal(0, Db.Count<Product>());
        }

        [Fact]
        public async Task Delete_UnknownKey_IsNoOpSuccess()
        {
            Db.AddProduct("Keep");
            var result = await Store.DeleteAsync(999);
            Assert.True(result.Success);
            Assert.Equal(1, Db.Count<Product>());
        }

        [Fact]
        public async Task Delete_ValidationFailure_KeepsAllRecords()
        {
            var a = Db.AddProduct("Deletable");
            var b = Db.AddProduct("Undeletable");
            var result = await Store.DeleteAsync(a, b);

            Assert.False(result.Success);
            Assert.Equal(DataMode.Delete, result.Mode);
            Assert.Contains(result.Exception.Errors, e => e.ErrorMessage == "This product cannot be deleted");
            Assert.Equal(2, Db.Count<Product>());
        }

        [Fact]
        public async Task Delete_NullArrays_Throw()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => Store.DeleteAsync((int[])null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => Store.DeleteAsync((ProductModel[])null));
        }

        [Fact]
        public async Task Delete_NonValidationError_ReturnsFailedResult()
        {
            var id = Db.AddProduct("Boom");
            var store = CreateStore(Db.DataLayer, new ThrowingValidator());
            var result = await store.DeleteAsync(id);
            Assert.False(result.Success);
            Assert.Equal(1, Db.Count<Product>());
        }

        [Fact]
        public async Task Create_NonValidationError_ReturnsFailedResult()
        {
            var store = CreateStore(Db.DataLayer, new ThrowingValidator());
            var result = await store.CreateAsync(New("Boom"));
            Assert.False(result.Success);
            Assert.Contains(result.Exception.Errors, e => e.ErrorMessage == "validator exploded");
            Assert.Equal(nameof(Product), result.Exception.Errors.Single().PropertyName);
            Assert.Equal(0, Db.Count<Product>());
        }

        private class ThrowingValidator : AbstractValidator<Product>
        {
            public ThrowingValidator() => RuleFor(p => p).Custom((p, c) => throw new InvalidOperationException("validator exploded"));
        }

        // ---------- GetByKey / Query ----------

        [Fact]
        public void GetByKey_ReturnsMappedModel()
        {
            var id = Db.AddProduct("Found", 7m, 1.5, 2.5);
            var model = Store.GetByKey(id);
            Assert.Equal(id, model.Id);
            Assert.Equal("Found", model.Name);
            Assert.Equal(7m, model.Price);
            Assert.Equal(1.5, model.Lat);
            Assert.Equal("saved:Found", model.Stamp);
        }

        [Fact]
        public void Query_ProjectsToModelAndFiltersServerSide()
        {
            Db.AddProduct("A", 1m);
            Db.AddProduct("B", 5m);
            Db.AddProduct("C", 10m);

            var list = Store.Query().Where(p => p.Price >= 5m).OrderBy(p => p.Name).ToList();
            Assert.Equal(new[] { "B", "C" }, list.Select(p => p.Name));
            Assert.Equal(3, Store.Query().Count());
        }

        protected void AssertQueryUsesStoreMapping()
        {
            var id = Db.AddProduct("Named");
            var item = Assert.Single(Store.Query<ProductNameModel>().ToList());
            Assert.Equal(id, item.Id);   // Id comes from Oid only through the configured mapping
            Assert.Equal("Named", item.Name);
            Assert.Equal(id, Store.Query().Single().Id);
        }

        [Fact]
        public async Task Query_SeesChangesMadeThroughStore()
        {
            Assert.Equal(0, Store.Query().Count());
            var m = New("First");
            await Store.CreateAsync(m);
            Assert.Equal("First", Store.Query().Single().Name);
            m.Name = "Renamed";
            await Store.UpdateAsync(m);
            Assert.Equal("Renamed", Store.Query().Single().Name);
            await Store.DeleteAsync(m);
            Assert.Equal(0, Store.Query().Count());
        }

        [Fact]
        public async Task Query_SupportsPredicateBuilderComposition()
        {
            Db.AddProduct("A", 1m);
            Db.AddProduct("B", 5m);
            Db.AddProduct("C", 10m);
            var filter = PredicateBuilder.False<ProductModel>()
                .Or(p => p.Name == "A")
                .Or(x => x.Price > 6m);
            var names = Store.Query().Where(filter).Select(p => p.Name).ToList().OrderBy(n => n);
            Assert.Equal(new[] { "A", "C" }, names);
            await Task.CompletedTask;
        }

        [Fact]
        public async Task ParallelCreates_AllPersist()
        {
            var tasks = Enumerable.Range(0, 40).Select(i => Task.Run(() => Store.CreateAsync(New("Par" + i)))).ToArray();
            var results = await Task.WhenAll(tasks);
            Assert.All(results, r => Assert.True(r.Success, r.Exception?.Message));
            Assert.Equal(40, Db.Count<Product>());
            Assert.Equal(40, results.Select(r => r.Data[0].Id).Distinct().Count());
        }
    }

    public class MapsterStoreTests : StoreTestsBase
    {
        protected override XPDataStore<int, ProductModel, Product> CreateStore(IDataLayer dataLayer, IValidator<Product> validator)
            => new ProductMapsterStore(dataLayer, Mappers.Mapster(), validator);

        [Fact]
        public void Query_UsesInjectedMapsterConfig() => AssertQueryUsesStoreMapping();

        [Fact]
        public void GetByKey_UnknownKey_ReturnsNull() => Assert.Null(Store.GetByKey(4242));

        [Fact]
        public async Task FindAsync_FiltersByCriteria()
        {
            Db.AddProduct("A", 1m);
            Db.AddProduct("B", 5m);
            var store = (ProductMapsterStore)Store;
            var found = await store.PublicFindAsync(CriteriaOperator.Parse("Price > 2"));
            Assert.Equal("B", Assert.Single(found).Name);
        }

        [Fact]
        public async Task FindAsync_AfterDispose_Throws()
        {
            var store = (ProductMapsterStore)Store;
            store.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => store.PublicFindAsync(CriteriaOperator.Parse("Price > 2")));
        }

        [Fact]
        public async Task FindAsync_NullFilter_Throws()
        {
            var store = (ProductMapsterStore)Store;
            await Assert.ThrowsAsync<ArgumentNullException>(() => store.PublicFindAsync(null));
        }

        [Fact]
        public void Dispose_DisposesSharedQueryUnitOfWork()
        {
            var store = (ProductMapsterStore)Store;
            var uow = store.CurrentUnitOfWork;
            store.Dispose();
            Assert.Throws<ObjectDisposedException>(() => uow.Query<Product>().Count());
        }
    }

    public class AutoMapperStoreTests : StoreTestsBase
    {
        protected override XPDataStore<int, ProductModel, Product> CreateStore(IDataLayer dataLayer, IValidator<Product> validator)
            => new ProductAutoMapperStore(dataLayer, Mappers.AutoMapper(), validator);

        [Fact]
        public void Query_UsesInjectedAutoMapperConfig() => AssertQueryUsesStoreMapping();

        [Fact]
        public void GetByKey_UnknownKey_ReturnsNull() => Assert.Null(Store.GetByKey(4242));
    }
}
