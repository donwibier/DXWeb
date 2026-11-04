using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DX.Data.Xpo.Tests
{
    /// <summary>
    /// XpoDatabase keeps its data layers by name for the whole process: every test uses a unique name
    /// </summary>
    public class XpoDatabaseTests
    {
        internal static string NewName() => "XpoDbTest" + Guid.NewGuid().ToString("N");

        internal static Action<XpoDatabaseOptions> InMemory(string name) => o =>
        {
            o.Name = name;
            o.ConnectionString = InMemoryDataStore.GetConnectionStringInMemory(true);
            o.UpdateSchema = AutoCreateOption.DatabaseAndSchema;
        };

        private static XpoDatabase NewDb() => new XpoDatabase(InMemory(NewName()));

        private static int CountProducts(XpoDatabase db) => db.Execute((d, s) => s.Query<Product>().Count(), false);

        // ---------- construction ----------

        [Fact]
        public void Ctor_NullActions_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new XpoDatabase((Action<XpoDatabaseOptions>[])null));
        }

        [Fact]
        public void Ctor_MissingConnectionString_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new XpoDatabase(o => o.Name = NewName()));
        }

        [Fact]
        public void Ctor_MissingName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new XpoDatabase(o => o.ConnectionString = InMemoryDataStore.GetConnectionStringInMemory(true)));
        }

        [Fact]
        public void Ctor_ExposesFirstOptions()
        {
            var name = NewName();
            var db = new XpoDatabase(InMemory(name));
            Assert.Equal(name, db.DataLayerName);
            Assert.Equal(InMemoryDataStore.GetConnectionStringInMemory(true), db.ConnectionString);
        }

        [Fact]
        public void SameName_SharesDataLayer()
        {
            var name = NewName();
            var a = new XpoDatabase(InMemory(name));
            var b = new XpoDatabase(InMemory(name));
            Assert.Same(XpoDatabase.GetDataLayer(new XpoDatabaseOptions { Name = name }), a.GetDataLayer(name));
            a.Execute((d, s) => { new Product(s) { Name = "shared" }; });
            Assert.Equal(1, CountProducts(b));
        }

        [Fact]
        public void StaticGetDataLayer_UnknownName_Throws()
        {
            Assert.Throws<Exception>(() => XpoDatabase.GetDataLayer(new XpoDatabaseOptions { Name = NewName() }));
            Assert.Throws<Exception>(() => XpoDatabase.GetUnitOfWork(NewName()));
        }

        [Fact(Skip = "BUG: XpoDatabase.GetDataLayer(string) (XpoDatabase.cs:145) ignores its argument and always returns the first data layer")]
        public void GetDataLayer_ByName_ReturnsThatLayer()
        {
            var first = NewName();
            var second = NewName();
            var db = new XpoDatabase(new[] { InMemory(first), InMemory(second) });
            Assert.NotSame(db.GetDataLayer(first), db.GetDataLayer(second));
            Assert.Same(XpoDatabase.GetDataLayer(new XpoDatabaseOptions { Name = second }), db.GetDataLayer(second));
        }

        [Fact]
        public void MultipleDatabases_ExecuteByName_IsolatesData()
        {
            var first = NewName();
            var second = NewName();
            var db = new XpoDatabase(new[] { InMemory(first), InMemory(second) });
            db.Execute(second, (d, s) => { new Product(s) { Name = "in second" }; });
            Assert.Equal(0, db.Execute(first, (d, s) => s.Query<Product>().Count()));
            Assert.Equal(1, db.Execute(second, (d, s) => s.Query<Product>().Count()));
        }

        [Fact(Skip = "BUG: XpoDatabase.GetDataLayer(options) (XpoDatabase.cs:221-226) is check-then-act; concurrent first use of one name creates several data layers (separate databases) and callers get different ones")]
        public async Task ConcurrentFirstUse_OfOneName_YieldsOneDataLayer()
        {
            for (int attempt = 0; attempt < 5; attempt++)
            {
                var name = NewName();
                var options = new XpoDatabaseOptions
                {
                    Name = name,
                    ConnectionString = InMemoryDataStore.GetConnectionStringInMemory(true),
                    UpdateSchema = AutoCreateOption.DatabaseAndSchema
                };
                using var barrier = new Barrier(8);
                var layers = Enumerable.Range(0, 8).Select(_ => Task.Run(() =>
                {
                    barrier.SignalAndWait();
                    return XpoDatabase.GetDataLayer(options);
                })).ToArray();
                var results = await Task.WhenAll(layers);
                Assert.Single(results.Distinct());
            }
        }

        [Fact]
        public void PlainXPObjectWithoutPersistentAttribute_IsUsable()
        {
            var db = NewDb();
            db.Execute((d, s) => { new PlainNote(s) { Text = "hello" }; });
            Assert.Equal("hello", db.Execute((d, s) => s.Query<PlainNote>().Single().Text));
        }

        // ---------- Execute ----------

        [Fact]
        public void Execute_CommitsByDefault()
        {
            var db = NewDb();
            db.Execute((d, s) =>
            {
                Assert.IsType<UnitOfWork>(s);
                Assert.Same(db, d);
                new Product(s) { Name = "x" };
            });
            Assert.Equal(1, CountProducts(db));
        }

        [Fact]
        public void Execute_CommitFalse_DiscardsChanges()
        {
            var db = NewDb();
            db.Execute((d, s) => { new Product(s) { Name = "x" }; }, true, false);
            Assert.Equal(0, CountProducts(db));
        }

        [Fact]
        public void Execute_Exception_PropagatesAndDiscardsChanges()
        {
            var db = NewDb();
            var ex = Assert.Throws<InvalidOperationException>(() => db.Execute((d, s) =>
            {
                new Product(s) { Name = "x" };
                throw new InvalidOperationException("boom");
            }));
            Assert.Equal("boom", ex.Message);
            Assert.Equal(0, CountProducts(db));
        }

        [Fact]
        public void Execute_DisposesSession()
        {
            var db = NewDb();
            Session captured = null;
            db.Execute((d, s) => { captured = s; });
            Assert.True(IsDisposed(captured));
        }

        private static bool IsDisposed(Session s)
        {
            try { s.Query<Product>().Count(); return false; }
            catch (ObjectDisposedException) { return true; }
        }

        [Fact]
        public void Execute_DisposesSession_EvenOnException()
        {
            var db = NewDb();
            Session captured = null;
            Assert.Throws<InvalidOperationException>(() => db.Execute((d, s) => { captured = s; throw new InvalidOperationException(); }));
            Assert.True(IsDisposed(captured));
        }

        [Fact]
        public void Execute_NonTransactional_UsesPlainSessionThatSavesImmediately()
        {
            var db = NewDb();
            db.Execute((d, s) =>
            {
                Assert.IsNotType<UnitOfWork>(s);
                var p = new Product(s) { Name = "direct" };
                p.Save();
            }, transactional: false);
            Assert.Equal(1, CountProducts(db));
        }

        [Fact]
        public void ExecuteOfT_ReturnsValueAndCommits()
        {
            var db = NewDb();
            var id = db.Execute((d, s) =>
            {
                var p = new Product(s) { Name = "x" };
                s.Save(p);
                return p;
            });
            Assert.Equal(1, CountProducts(db));
            Assert.NotNull(id);
        }

        [Fact]
        public async Task ExecuteAsync_Action_Commits()
        {
            var db = NewDb();
            await db.ExecuteAsync((d, s) => { new Product(s) { Name = "x" }; });
            Assert.Equal(1, CountProducts(db));
        }

        [Fact]
        public async Task ExecuteAsync_Func_ReturnsValue()
        {
            var db = NewDb();
            var name = await db.ExecuteAsync((d, s) => { new Product(s) { Name = "x" }; return "done"; });
            Assert.Equal("done", name);
            Assert.Equal(1, CountProducts(db));
        }

        [Fact]
        public async Task ExecuteAsync_Exception_SurfacesOnAwaitAndRollsBack()
        {
            var db = NewDb();
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                db.ExecuteAsync((d, s) => { new Product(s) { Name = "x" }; throw new InvalidOperationException(); }));
            Assert.Equal(0, CountProducts(db));
        }

        [Fact]
        public void ExecuteAsyncOfT_RunsSynchronouslyOnCallerThread()
        {
            // documents the current behaviour: ExecuteAsync<T> is Task.FromResult(Execute(...)), the work runs before
            // the Task is even returned, on the calling thread
            var db = NewDb();
            var caller = Environment.CurrentManagedThreadId;
            int worker = -1;
            var task = db.ExecuteAsync((d, s) => { worker = Environment.CurrentManagedThreadId; return 1; });
            Assert.True(task.IsCompleted);
            Assert.Equal(caller, worker);
        }

        [Fact(Skip = "BUG: same DataStorePool root cause (XpoDatabase.cs:271): parallel ExecuteAsync on an in-memory XpoDatabase throws SchemaCorrectionNeededException / writes to separate databases")]
        public async Task ExecuteAsync_ParallelCalls_AllCommit()
        {
            var db = NewDb();
            var tasks = Enumerable.Range(0, 50).Select(i =>
                db.ExecuteAsync((d, s) => { new Product(s) { Name = "p" + i }; })).ToArray();
            await Task.WhenAll(tasks);
            Assert.Equal(50, CountProducts(db));
            var names = db.Execute((d, s) => s.Query<Product>().Select(p => p.Name).ToList(), false);
            Assert.Equal(50, names.Distinct().Count());
        }

        [Fact(Skip = "BUG: XpoDatabase.CreateDataLayer (XpoDatabase.cs:271) wraps an InMemoryDataSet connection in a DataStorePool; every extra pooled connection is a new empty in-memory database, so concurrent Execute fails with SchemaCorrectionNeededException")]
        public async Task ExecuteAsync_ConcurrentReads_SeeTheSameDatabase()
        {
            var db = NewDb();
            db.Execute((d, s) => { new Product(s) { Name = "warm" }; });
            var counts = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => Task.Run(() =>
                db.Execute((d, s) => { Thread.Sleep(20); return s.Query<Product>().Count(); }, false))));
            Assert.All(counts, c => Assert.Equal(1, c));
        }

        [Fact(Skip = "BUG: constructing an XpoDatabase with DatabaseAndSchema replaces the process-wide XpoDefault.DataLayer (XpoDatabase.cs:286), so new Session()/new UnitOfWork() silently use the last database created")]
        public void Ctor_DoesNotReplaceXpoDefaultDataLayer()
        {
            var before = XpoDefault.DataLayer;
            NewDb();
            Assert.Same(before, XpoDefault.DataLayer);
        }

        [Fact]
        public void GetUnitOfWorkAndSession_UseNamedLayer()
        {
            var name = NewName();
            var db = new XpoDatabase(InMemory(name));
            using (var uow = db.GetUnitOfWork())
            {
                new Product(uow) { Name = "uow" };
                uow.CommitChanges();
            }
            using var s = db.GetSession();
            Assert.Equal(1, s.Query<Product>().Count());
            Assert.Same(XpoDatabase.GetDataLayer(new XpoDatabaseOptions { Name = name }), s.DataLayer);
        }

        // ---------- cloning ----------

        [Fact]
        public void CloneCollection_CopiesMatchingObjectsToTarget()
        {
            var source = NewDb();
            var target = NewDb();
            source.Execute((d, s) =>
            {
                new Product(s) { Name = "a", Price = 1 };
                new Product(s) { Name = "b", Price = 2 };
                new Product(s) { Name = "c", Price = 3 };
            });

            var clones = source.CloneCollection<Product>(CriteriaOperator.Parse("Price >= 2"),
                new[] { new SortProperty("Name", DevExpress.Xpo.DB.SortingDirection.Ascending) }, target, synchronize: false);

            Assert.Equal(new[] { "b", "c" }, clones.Select(c => c.Name));
            Assert.Equal(new[] { "b", "c" }, target.Execute((d, s) => s.Query<Product>().OrderBy(p => p.Name).Select(p => p.Name).ToList()));
        }

        [Fact]
        public void Clone_Synchronize_UpdatesExistingTargetObject()
        {
            var source = NewDb();
            var target = NewDb();
            target.Execute((d, s) => { new Product(s) { Name = "old" }; });
            source.Execute((d, s) => { new Product(s) { Name = "new", Price = 9 }; });

            using var src = source.GetSession();
            var p = src.Query<Product>().Single();
            source.Clone(p, target, synchronize: true);

            var names = target.Execute((d, s) => s.Query<Product>().Select(x => x.Name).ToList());
            Assert.Equal(new[] { "new" }, names);
        }

        [Fact]
        public void Clone_Null_Throws()
        {
            var db = NewDb();
            Assert.Throws<ArgumentNullException>(() => db.Clone<Product>(null, db));
            using var s = db.GetSession();
            Assert.Throws<ArgumentNullException>(() => db.CloneCollection<Product>(null, null, (XpoDatabase)null));
        }

        // ---------- DI extensions ----------

        [Fact]
        public void AddXpoDatabase_RegistersSingleton()
        {
            var name = NewName();
            using var sp = new ServiceCollection().AddXpoDatabase(InMemory(name)).BuildServiceProvider();
            var a = sp.GetRequiredService<XpoDatabase>();
            Assert.Same(a, sp.GetRequiredService<XpoDatabase>());
            Assert.Equal(name, a.DataLayerName);
        }

        [Fact]
        public void AddXpoDatabase_ByConnectionName_ReadsConfiguration()
        {
            var name = NewName();
            var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:" + name] = InMemoryDataStore.GetConnectionStringInMemory(true)
            }).Build();
            var services = new ServiceCollection().AddSingleton<IConfiguration>(cfg);
#pragma warning disable CS0618
            services.AddXpoDatabase(name);
#pragma warning restore CS0618
            using var sp = services.BuildServiceProvider();
            Assert.Equal(name, sp.GetRequiredService<XpoDatabase>().DataLayerName);
        }

        [Fact]
        public void AddXpoUnitOfWork_IsScoped()
        {
            using var sp = new ServiceCollection().AddXpoDatabase(InMemory(NewName())).AddXpoUnitOfWork("ignored").BuildServiceProvider();
            using var s1 = sp.CreateScope();
            using var s2 = sp.CreateScope();
            Assert.Same(s1.ServiceProvider.GetRequiredService<UnitOfWork>(), s1.ServiceProvider.GetRequiredService<UnitOfWork>());
            Assert.NotSame(s1.ServiceProvider.GetRequiredService<UnitOfWork>(), s2.ServiceProvider.GetRequiredService<UnitOfWork>());
        }

        [Fact(Skip = "BUG: AddXpoUnitOfWork(connectionName) (XpoCoreExtensions.cs:38) ignores connectionName and always binds the first database")]
        public void AddXpoUnitOfWork_UsesNamedConnection()
        {
            var first = NewName();
            var second = NewName();
            using var sp = new ServiceCollection()
                .AddXpoDatabases(new[] { InMemory(first), InMemory(second) })
                .AddXpoUnitOfWork(second).BuildServiceProvider();
            using var scope = sp.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<UnitOfWork>();
            Assert.Same(XpoDatabase.GetDataLayer(new XpoDatabaseOptions { Name = second }), uow.DataLayer);
        }
    }
}
