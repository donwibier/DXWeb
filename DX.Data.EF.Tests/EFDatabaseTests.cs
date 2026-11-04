using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DX.Data.EF.Tests
{
    // EFDatabase creates contexts via "new TEFContext()", which uses the static TestDbContext.DefaultConnection.
    // All tests that touch it live in this one class/collection so they never run concurrently.
    [Collection(nameof(EFDatabaseTests))]
    public class EFDatabaseTests : IDisposable
    {
        readonly SqliteDb db = new SqliteDb();
        readonly EFDatabase<TestDbContext> sut = new EFDatabase<TestDbContext>();

        public EFDatabaseTests() { TestDbContext.DefaultConnection = db.Connection; }
        public void Dispose() { TestDbContext.DefaultConnection = null; db.Dispose(); }

        [Fact]
        public void Execute_commits_tracked_changes_by_default()
        {
            sut.Execute((d, ctx, tx) => { ctx.Customers.Add(new Customer { Name = "A" }); });
            Assert.Single(db.AllCustomers());
        }

        [Fact]
        public void Execute_with_commit_false_rolls_back()
        {
            sut.Execute((d, ctx, tx) =>
            {
                ctx.Customers.Add(new Customer { Name = "A" });
                ctx.SaveChanges();
            }, transactional: true, commit: false);
            Assert.Empty(db.AllCustomers());
        }

        [Fact(Skip = "BUG: transactional:false still opens a transaction and never commits it, so the work's own SaveChanges is rolled back (EFDatabase.cs:28,32 and 55,59)")]
        public void Execute_non_transactional_keeps_saved_changes()
        {
            sut.Execute((d, ctx, tx) =>
            {
                ctx.Customers.Add(new Customer { Name = "A" });
                ctx.SaveChanges();
            }, transactional: false);
            Assert.Single(db.AllCustomers());
        }

        [Fact]
        public void Execute_T_returns_result_and_commits()
        {
            var id = sut.Execute((d, ctx, tx) =>
            {
                var c = new Customer { Name = "A" };
                ctx.Customers.Add(c);
                ctx.SaveChanges();
                return c.Id;
            });
            Assert.Equal(id, Assert.Single(db.AllCustomers()).Id);
        }

        [Fact]
        public void Execute_exception_in_work_rolls_back_and_propagates()
        {
            Assert.Throws<InvalidOperationException>(() => sut.Execute((d, ctx, tx) =>
            {
                ctx.Customers.Add(new Customer { Name = "A" });
                ctx.SaveChanges();
                throw new InvalidOperationException("boom");
            }));
            Assert.Empty(db.AllCustomers());
        }

        [Fact]
        public void Execute_work_that_commits_itself_throws_on_second_commit()
        {
            // Documents current behaviour: the work may not commit the provided transaction itself
            // when commit:true, because Execute unconditionally commits again afterwards.
            var ex = Record.Exception(() => sut.Execute((d, ctx, tx) =>
            {
                ctx.Customers.Add(new Customer { Name = "A" });
                ctx.SaveChanges();
                tx.Commit();
            }));
            Assert.NotNull(ex);
            Assert.Single(db.AllCustomers());
        }

        [Fact]
        public async Task ExecuteAsync_T_returns_result()
        {
            var n = await sut.ExecuteAsync((d, ctx, tx) => ctx.Customers.Count());
            Assert.Equal(0, n);
        }

        [Fact]
        public void ExecuteAsync_T_runs_synchronously_on_caller_thread()
        {
            // Documents a sync-over-async design issue: ExecuteAsync<T> is Task.FromResult(Execute(...)).
            int callerThread = Environment.CurrentManagedThreadId, workThread = -1;
            var t = sut.ExecuteAsync((d, ctx, tx) => { workThread = Environment.CurrentManagedThreadId; return 1; });
            Assert.True(t.IsCompleted);
            Assert.Equal(callerThread, workThread);
        }

        [Fact]
        public async Task ExecuteAsync_action_commits()
        {
            await sut.ExecuteAsync((d, ctx, tx) => { ctx.Customers.Add(new Customer { Name = "A" }); });
            Assert.Single(db.AllCustomers());
        }
    }

    public class TransactionalExecTests : IDisposable
    {
        readonly SqliteDb db = new SqliteDb();
        readonly TestDbContext ctx;
        readonly MapsterCustomerStore store;

        public TransactionalExecTests()
        {
            ctx = db.NewContext();
            store = new MapsterCustomerStore(ctx, Mappers.Mapster(), new CustomerValidator());
        }
        public void Dispose() { ctx.Dispose(); db.Dispose(); }

        [Fact]
        public async Task TransactionalExec_saves_and_commits_pending_changes()
        {
            await store.RunInTransaction(c => { c.Customers.Add(new Customer { Name = "A" }); return Task.FromResult(0); });
            Assert.Single(db.AllCustomers());
        }

        [Fact(Skip = "BUG: TransactionalExecAsync only commits when ChangeTracker.HasChanges(); work that already called SaveChanges is silently rolled back (EFDataStore.cs:86/101)")]
        public async Task TransactionalExec_commits_when_work_already_saved()
        {
            await store.RunInTransaction(async c =>
            {
                c.Customers.Add(new Customer { Name = "A" });
                await c.SaveChangesAsync();
                return 0;
            });
            Assert.Single(db.AllCustomers());
        }

        [Fact]
        public async Task TransactionalExec_autoCommit_false_rolls_back()
        {
            await store.RunInTransaction(c => { c.Customers.Add(new Customer { Name = "A" }); return Task.FromResult(0); }, autoCommit: false);
            Assert.Empty(db.AllCustomers());
        }
    }

    public class DataResultFromStoreTests : IDisposable
    {
        readonly SqliteDb db = new SqliteDb();
        readonly TestDbContext ctx;
        readonly MapsterCustomerStore store;

        public DataResultFromStoreTests()
        {
            ctx = db.NewContext();
            store = new MapsterCustomerStore(ctx, Mappers.Mapster(), new CustomerValidator());
        }
        public void Dispose() { ctx.Dispose(); db.Dispose(); }

        [Fact]
        public async Task Create_db_error_property_name_is_entity_type_name()
        {
            db.Seed("Alice");
            var r = await store.CreateAsync(new CustomerDto { Name = "Alice" });
            Assert.Equal(nameof(Customer), Assert.Single(r.Exception.Errors).PropertyName);
        }

        [Fact]
        public void Test_method_opens_and_commits_an_empty_transaction()
        {
            // Public leftover debug method on the store (EFDataStore.cs:64); ensure it is at least harmless.
            store.Test();
            Assert.Null(ctx.Database.CurrentTransaction);
        }
    }
}
