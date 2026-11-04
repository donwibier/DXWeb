using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace DX.Data.EF.Tests
{
    /// <summary>Records the text of every DbCommand executed through the context and counts SaveChanges calls.</summary>
    public sealed class CommandRecorder : DbCommandInterceptor, ISaveChangesInterceptor
    {
        public List<string> Commands { get; } = new List<string>();
        public int SaveChangesCalls { get; private set; }
        public bool Enabled { get; set; }

        public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        { if (Enabled) SaveChangesCalls++; return result; }
        public ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        { if (Enabled) SaveChangesCalls++; return new ValueTask<InterceptionResult<int>>(result); }

        void Record(DbCommand command)
        {
            if (Enabled)
                lock (Commands) Commands.Add(command.CommandText);
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        { Record(command); return result; }
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        { Record(command); return new ValueTask<InterceptionResult<DbDataReader>>(result); }
        public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        { Record(command); return result; }
        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        { Record(command); return new ValueTask<InterceptionResult<int>>(result); }
        public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
        { Record(command); return result; }
        public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
        { Record(command); return new ValueTask<InterceptionResult<object>>(result); }

        public int Count(string verb) => Commands.Sum(c => CountOccurrences(c, verb));

        static int CountOccurrences(string text, string verb)
        {
            int n = 0, i = 0;
            while ((i = text.IndexOf(verb, i, System.StringComparison.Ordinal)) >= 0) { n++; i += verb.Length; }
            return n;
        }
    }

    /// <summary>Regression tests for the round-trip / change-tracker behaviour of EFDataStore.</summary>
    public class PerfTests : System.IDisposable
    {
        const int N = 10;
        readonly SqliteDb db = new SqliteDb();
        readonly CommandRecorder recorder = new CommandRecorder();
        readonly TestDbContext ctx;
        readonly MapsterCustomerStore store;
        readonly ITestOutputHelper output;

        public PerfTests(ITestOutputHelper output)
        {
            this.output = output;
            ctx = new TestDbContext(db.Connection, interceptor: recorder);
            store = new MapsterCustomerStore(ctx, Mappers.Mapster(), new CustomerValidator());
        }
        public void Dispose() { ctx.Dispose(); db.Dispose(); }

        void Dump()
        {
            output.WriteLine($"SaveChanges={recorder.SaveChangesCalls}, {recorder.Commands.Count} commands, SELECT={recorder.Count("SELECT")}, INSERT={recorder.Count("INSERT")}, UPDATE={recorder.Count("UPDATE")}, DELETE={recorder.Count("DELETE")}");
            foreach (var c in recorder.Commands) output.WriteLine(c);
        }

        static CustomerDto Dto(int i, int id = 0, int score = 1)
            => new CustomerDto { Id = id, Name = "C" + i, Email = "c" + i + "@test", Score = score };

        [Fact]
        public async Task Create_batch_saves_once()
        {
            recorder.Enabled = true;
            var r = await store.CreateAsync(Enumerable.Range(0, N).Select(i => Dto(i)).ToArray());
            Dump();

            Assert.True(r.Success, r.Exception?.Message);
            Assert.Equal(N, db.AllCustomers().Count);
            Assert.Equal(N, recorder.Count("INSERT"));
            Assert.Equal(0, recorder.Count("UPDATE"));
            Assert.Equal(1, recorder.SaveChangesCalls);
        }

        [Fact]
        public async Task Update_batch_loads_all_keys_in_one_query()
        {
            var seeded = Enumerable.Range(0, N).Select(i => db.Seed("C" + i, score: 1)).ToList();
            recorder.Enabled = true;
            var r = await store.UpdateAsync(seeded.Select((c, i) => Dto(i, c.Id, score: 100 + i)).ToArray());
            Dump();

            Assert.True(r.Success, r.Exception?.Message);
            Assert.Equal(Enumerable.Range(100, N), db.AllCustomers().Select(c => c.Score));
            Assert.Equal(1, recorder.Count("SELECT"));
            Assert.Equal(N, recorder.Count("UPDATE"));
            Assert.Equal(1, recorder.SaveChangesCalls);
        }

        [Fact]
        public async Task Delete_batch_loads_all_keys_in_one_query()
        {
            var ids = Enumerable.Range(0, N).Select(i => db.Seed("C" + i).Id).ToArray();
            recorder.Enabled = true;
            var r = await store.DeleteAsync(ids);
            Dump();

            Assert.True(r.Success, r.Exception?.Message);
            Assert.Empty(db.AllCustomers());
            Assert.Equal(1, recorder.Count("SELECT"));
            Assert.Equal(N, recorder.Count("DELETE"));
            Assert.Equal(1, recorder.SaveChangesCalls);
        }

        [Fact]
        public async Task Update_writes_only_changed_columns()
        {
            var a = db.Seed("Alice", score: 1, email: "a@test");
            recorder.Enabled = true;
            var r = await store.UpdateAsync(new CustomerDto { Id = a.Id, Name = "Alice", Email = "a@test", Score = 5 });
            Dump();

            Assert.True(r.Success, r.Exception?.Message);
            var update = Assert.Single(recorder.Commands, c => c.Contains("UPDATE"));
            Assert.Contains("\"Score\"", update);
            Assert.DoesNotContain("\"Name\"", update);
            Assert.DoesNotContain("\"Email\"", update);
        }

        [Fact]
        public async Task Update_with_no_changes_issues_no_update()
        {
            var a = db.Seed("Alice", score: 1, email: "a@test");
            recorder.Enabled = true;
            var r = await store.UpdateAsync(new CustomerDto { Id = a.Id, Name = "Alice", Email = "a@test", Score = 1 });
            Dump();

            Assert.True(r.Success, r.Exception?.Message);
            Assert.Equal(0, recorder.Count("UPDATE"));
        }

        [Fact]
        public void GetByKey_does_not_track()
        {
            var a = db.Seed("Alice");
            var dto = store.GetByKey(a.Id);
            Assert.Equal("Alice", dto.Name);
            Assert.Empty(ctx.ChangeTracker.Entries());
        }

        [Fact]
        public async Task GetByKey_then_update_on_same_store_succeeds()
        {
            var a = db.Seed("Alice", score: 1);
            var dto = store.GetByKey(a.Id);
            dto.Score = 9;
            var r = await store.UpdateAsync(dto);
            Assert.True(r.Success, r.Exception?.Message);
            Assert.Equal(9, Assert.Single(db.AllCustomers()).Score);
            // a second read reflects the persisted state
            Assert.Equal(9, store.GetByKey(a.Id).Score);
        }

        [Fact]
        public async Task Successful_and_failed_operations_leave_no_pending_changes()
        {
            db.Seed("Alice");
            Assert.False((await store.CreateAsync(Dto(1), new CustomerDto { Name = "Alice" })).Success);
            Assert.False(ctx.ChangeTracker.HasChanges());
            Assert.True((await store.CreateAsync(Dto(2))).Success);
            Assert.False(ctx.ChangeTracker.HasChanges());
        }

        [Fact]
        public async Task Update_batch_larger_than_key_chunk_uses_one_query_per_64_keys()
        {
            var seeded = Enumerable.Range(0, 70).Select(i => db.Seed("C" + i, score: 1)).ToList();
            recorder.Enabled = true;
            var r = await store.UpdateAsync(seeded.Select((c, i) => Dto(i, c.Id, score: 2)).ToArray());

            Assert.True(r.Success, r.Exception?.Message);
            Assert.All(db.AllCustomers(), c => Assert.Equal(2, c.Score));
            Assert.Equal(2, recorder.Count("SELECT"));
        }

        [Fact]
        public async Task Update_still_persists_when_auto_detect_changes_is_disabled()
        {
            var a = db.Seed("Alice", score: 1);
            ctx.ChangeTracker.AutoDetectChangesEnabled = false;
            var r = await store.UpdateAsync(new CustomerDto { Id = a.Id, Name = "Alice", Email = "alice@test", Score = 3 },
                new CustomerDto { Id = db.Seed("Bob").Id, Name = "Bob", Email = "bob@test", Score = 4 });
            Assert.True(r.Success, r.Exception?.Message);
            Assert.Equal(new[] { 3, 4 }, db.AllCustomers().Select(c => c.Score));
        }

        [Fact]
        public async Task Overridden_EFGetByKey_is_still_used_for_reads_updates_and_deletes()
        {
            var custom = new CustomLookupStore(ctx);
            var a = db.Seed("Alice", score: 1);
            var b = db.Seed("Bob", score: 1);

            Assert.Equal("Alice", custom.GetByKey(a.Id).Name);
            Assert.True((await custom.UpdateAsync(
                new CustomerDto { Id = a.Id, Name = "Alice", Email = "a@test", Score = 5 },
                new CustomerDto { Id = b.Id, Name = "Bob", Email = "b@test", Score = 6 })).Success);
            Assert.Equal(3, custom.Lookups); // 1 read + 2 updates
            Assert.Equal(new[] { 5, 6 }, db.AllCustomers().Select(c => c.Score));

            // fresh context: the untracked lookup would otherwise collide with the entities attached by the update
            using (var ctx2 = db.NewContext())
            {
                var custom2 = new CustomLookupStore(ctx2);
                var r = await custom2.DeleteAsync(a.Id, b.Id);
                Assert.True(r.Success, r.Exception?.Message);
                Assert.Equal(2, custom2.Lookups);
            }
            Assert.Empty(db.AllCustomers());
        }

        /// <summary>A store with a custom (untracked) EFGetByKey: the built-in key queries must not bypass it.</summary>
        class CustomLookupStore : Mapster.EFMapsterDataStore<TestDbContext, int, CustomerDto, Customer>
        {
            public CustomLookupStore(TestDbContext ctx) : base(ctx, Mappers.Mapster(), new CustomerValidator()) { }
            public int Lookups { get; private set; }
            public override string KeyField => nameof(CustomerDto.Id);
            public override int ModelKey(CustomerDto model) => model.Id;
            public override void SetModelKey(CustomerDto model, int key) => model.Id = key;
            protected override int DBModelKey(Customer model) => model.Id;
            protected override Customer EFGetByKey(int key)
            {
                Lookups++;
                return Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AsNoTracking(DbContext.Customers)
                    .FirstOrDefault(c => c.Id == key);
            }
        }
    }
}
