using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using FluentValidation;
using Xunit;

namespace DX.Data.Xpo.Tests
{
    /// <summary>A store that lets a test run work in the delete's session just before each object is deleted</summary>
    public class HookedProductStore : ProductMapsterStore
    {
        public HookedProductStore(IDataLayer dataLayer, IValidator<Product> validator) : base(dataLayer, Mappers.Mapster(), validator) { }

        public Func<Product, Session, Task> BeforeDelete = (p, s) => Task.FromResult(0);
        public List<Type> DeleteSessionTypes { get; } = new List<Type>();

        protected override async Task BeforeDeleteAsync(Product dbModel, Session session)
        {
            DeleteSessionTypes.Add(session.GetType());
            await BeforeDelete(dbModel, session);
        }
    }

    /// <summary>
    /// DeleteAsync on a pooled (SQL) database runs in a database level transaction on a connection of its own:
    /// work flushed in BeforeDeleteAsync is rolled back with a failed delete, and other requests do not see it
    /// </summary>
    public sealed class ExplicitDeleteTests : IDisposable
    {
        private readonly string _file = Path.Combine(Path.GetTempPath(), "dxdata-explicit-" + Guid.NewGuid().ToString("N") + ".db");
        private readonly IDataLayer _layer;

        public ExplicitDeleteTests()
        {
            var name = "Explicit" + Guid.NewGuid().ToString("N");
            var db = new XpoDatabase(o =>
            {
                o.Name = name;
                o.ConnectionString = "XpoProvider=SQLite;Data Source=" + _file;
                o.UpdateSchema = AutoCreateOption.DatabaseAndSchema;
            });
            _layer = db.GetDataLayer(name);
        }

        public void Dispose()
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            try { File.Delete(_file); } catch (IOException) { }
        }

        private int Add<T>(Func<UnitOfWork, T> create) where T : XPObject
        {
            using var uow = new UnitOfWork(_layer);
            var o = create(uow);
            uow.CommitChanges();
            return o.Oid;
        }

        private int Count<T>() where T : class
        {
            using var uow = new UnitOfWork(_layer);
            return uow.Query<T>().Count();
        }

        [Fact]
        public async Task Delete_OnAPooledDatabase_RunsInAnExplicitUnitOfWork()
        {
            var id = Add(u => new Product(u) { Name = "A", Price = 1 });
            var store = new HookedProductStore(_layer, new ProductValidator());

            var result = await store.DeleteAsync(id);

            Assert.True(result.Success, result.Exception?.Message);
            Assert.Equal(new[] { typeof(ExplicitUnitOfWork) }, store.DeleteSessionTypes);
            Assert.Equal(0, Count<Product>());
        }

        [Fact]
        public async Task Delete_Failing_RollsBackWorkFlushedBeforeTheDelete()
        {
            var ok = Add(u => new Product(u) { Name = "A", Price = 1 });
            var undeletable = Add(u => new Product(u) { Name = "Undeletable", Price = 1 });
            Add(u => new PlainNote(u) { Text = "link" });
            var store = new HookedProductStore(_layer, new ProductValidator());
            store.BeforeDelete = async (p, s) =>
            {
                // as the identity stores do with association links: delete and flush ahead of the object itself
                await s.DeleteAsync(s.Query<PlainNote>().ToList());
                s.FlushChanges();
            };

            var result = await store.DeleteAsync(ok, undeletable);

            Assert.False(result.Success);
            Assert.Equal(2, Count<Product>());
            Assert.Equal(1, Count<PlainNote>());
        }

        [Fact]
        public async Task Delete_FlushedWork_IsNotSeenByOtherRequestsBeforeTheCommit()
        {
            var id = Add(u => new Product(u) { Name = "A", Price = 1 });
            Add(u => new PlainNote(u) { Text = "link" });
            var store = new HookedProductStore(_layer, new ProductValidator());
            int seenByOthers = -1;
            store.BeforeDelete = async (p, s) =>
            {
                await s.DeleteAsync(s.Query<PlainNote>().ToList());
                s.FlushChanges();
                // another request, on the shared data layer, while the delete's transaction is open
                seenByOthers = await Task.Run(() => Count<PlainNote>());
            };

            var result = await store.DeleteAsync(id);

            Assert.True(result.Success, result.Exception?.Message);
            Assert.Equal(1, seenByOthers);
            Assert.Equal(0, Count<PlainNote>());
            Assert.Equal(0, Count<Product>());
        }

        [Fact]
        public async Task Delete_ManyTimes_ReturnsItsConnectionsToThePool()
        {
            var ids = Enumerable.Range(0, 30).Select(i => Add(u => new Product(u) { Name = "P" + i, Price = 1 })).ToList();
            var store = new HookedProductStore(_layer, new ProductValidator());

            foreach (var id in ids)
                Assert.True((await store.DeleteAsync(id)).Success);

            Assert.Equal(0, Count<Product>());
        }

        [Fact]
        public async Task Delete_OnAnInMemoryDatabase_UsesAUnitOfWork()
        {
            var db = new InMemoryDb();
            var id = db.AddProduct("A", 1);
            var store = new HookedProductStore(db.DataLayer, new ProductValidator());

            var result = await store.DeleteAsync(id);

            Assert.True(result.Success, result.Exception?.Message);
            Assert.Equal(new[] { typeof(UnitOfWork) }, store.DeleteSessionTypes);
        }
    }
}
