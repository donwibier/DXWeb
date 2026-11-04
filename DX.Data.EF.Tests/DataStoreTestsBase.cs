using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DX.Data.EF.Tests
{
    /// <summary>
    /// Behavioural tests for EFDataStore, executed once per mapper flavour (Mapster / AutoMapper).
    /// </summary>
    public abstract class DataStoreTestsBase : IDisposable
    {
        protected readonly SqliteDb Db = new SqliteDb();
        protected readonly CustomerValidator Validator = new CustomerValidator();
        protected readonly TestDbContext Ctx;
        protected readonly IQueryableDataStore<int, CustomerDto> Store;

        protected DataStoreTestsBase()
        {
            Ctx = Db.NewContext();
            Store = CreateStore(Ctx, Validator);
        }

        protected abstract IQueryableDataStore<int, CustomerDto> CreateStore(TestDbContext ctx, CustomerValidator validator);

        public void Dispose()
        {
            Ctx.Dispose();
            Db.Dispose();
        }

        static CustomerDto Dto(string name, int score = 1, int id = 0)
            => new CustomerDto { Id = id, Name = name, Email = name.ToLower() + "@test", Score = score };

        // ---------------------------------------------------------------- Create

        [Fact]
        public async Task Create_single_item_inserts_and_sets_key()
        {
            var dto = Dto("Alice");
            var r = await Store.CreateAsync(dto);

            Assert.True(r.Success);
            Assert.Equal(DataMode.Create, r.Mode);
            Assert.Null(r.Exception);
            Assert.NotEqual(0, dto.Id);
            Assert.Same(dto, Assert.Single(r.Data));
            var row = Assert.Single(Db.AllCustomers());
            Assert.Equal(dto.Id, row.Id);
            Assert.Equal("Alice", row.Name);
        }

        [Fact]
        public async Task Create_passes_mode_and_store_to_validator()
        {
            await Store.CreateAsync(Dto("Alice"));
            Assert.Equal(DataMode.Create, Assert.Single(Validator.SeenModes));
            Assert.Same(Store, Assert.Single(Validator.SeenStores));
        }

        [Fact]
        public async Task Create_invalid_item_returns_validation_errors_and_inserts_nothing()
        {
            var r = await Store.CreateAsync(Dto("  ", score: -1));

            Assert.False(r.Success);
            Assert.NotNull(r.Exception);
            Assert.Contains(r.Exception.Errors, e => e.PropertyName == "Name");
            Assert.Contains(r.Exception.Errors, e => e.PropertyName == "Score");
            Assert.Empty(Db.AllCustomers());
        }

        [Fact]
        public async Task Create_with_nonzero_key_inserts_with_that_key()
        {
            var r = await Store.CreateAsync(Dto("Alice", id: 42));
            Assert.True(r.Success);
            Assert.Equal(42, Assert.Single(Db.AllCustomers()).Id);
        }

        [Fact]
        public async Task Create_batch_inserts_all_items()
        {
            var items = new[] { Dto("A"), Dto("B"), Dto("C") };
            var r = await Store.CreateAsync(items);

            Assert.True(r.Success);
            Assert.Equal(3, Db.AllCustomers().Count);
            Assert.All(items, i => Assert.NotEqual(0, i.Id));
        }

        [Fact]
        public async Task Update_batch_updates_all_items()
        {
            var a = Db.Seed("A");
            var b = Db.Seed("B");
            var r = await Store.UpdateAsync(
                new CustomerDto { Id = a.Id, Name = "A", Score = 10 },
                new CustomerDto { Id = b.Id, Name = "B", Score = 20 });

            Assert.True(r.Success);
            Assert.Equal(new[] { 10, 20 }, Db.AllCustomers().Select(c => c.Score));
        }

        [Fact]
        public async Task Create_batch_result_contains_all_items()
        {
            var items = new[] { Dto("A"), Dto("B"), Dto("C") };
            var r = await Store.CreateAsync(items);
            Assert.Equal(items, r.Data);
        }

        [Fact]
        public async Task Create_batch_with_one_invalid_item_is_atomic_and_returns_failure()
        {
            var r = await Store.CreateAsync(Dto("A"), Dto("", score: 1));

            Assert.False(r.Success);
            Assert.Empty(Db.AllCustomers());
        }

        [Fact]
        public async Task Create_empty_batch_returns_empty_data()
        {
            var r = await Store.CreateAsync();
            Assert.True(r.Success);
            Assert.NotNull(r.Data);
            Assert.Empty(r.Data);
        }

        [Fact]
        public async Task Create_batch_db_error_rolls_back_and_reports_failing_item()
        {
            Db.Seed("Alice");
            var dup = Dto("Alice");
            var r = await Store.CreateAsync(Dto("A"), dup, Dto("B"));

            Assert.False(r.Success);
            Assert.Equal(DataMode.Create, r.Mode);
            Assert.Same(dup, Assert.Single(r.Data));
            Assert.Equal(new[] { "Alice" }, Db.AllCustomers().Select(c => c.Name));
            Assert.False(Ctx.ChangeTracker.HasChanges());

            Assert.True((await Store.CreateAsync(Dto("C"))).Success);
            Assert.Equal(new[] { "Alice", "C" }, Db.AllCustomers().Select(c => c.Name));
        }

        [Fact]
        public async Task Store_mixed_batch_creates_and_updates_and_returns_all_items()
        {
            var a = Db.Seed("Alice", score: 1);
            var items = new[] { new CustomerDto { Id = a.Id, Name = "Alice", Email = "a@test", Score = 4 }, Dto("Bob") };
            var r = await Store.StoreAsync(items);

            Assert.True(r.Success, r.Exception?.Message);
            Assert.Equal(items, r.Data);
            Assert.NotEqual(0, items[1].Id);
            Assert.Equal(new[] { 4, 1 }, Db.AllCustomers().Select(c => c.Score));
        }

        [Fact]
        public async Task Update_empty_batch_returns_update_mode_and_empty_data()
        {
            var r = await Store.UpdateAsync();
            Assert.True(r.Success);
            Assert.Equal(DataMode.Update, r.Mode);
            Assert.Empty(r.Data);
        }

        [Fact]
        public async Task Create_null_array_throws_ArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => Store.CreateAsync((CustomerDto[])null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => Store.UpdateAsync((CustomerDto[])null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => Store.StoreAsync((CustomerDto[])null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => Store.DeleteAsync((CustomerDto[])null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => Store.DeleteAsync((int[])null));
        }

        [Fact]
        public async Task Create_duplicate_unique_value_returns_failure_with_db_message()
        {
            Db.Seed("Alice");
            var r = await Store.CreateAsync(Dto("Alice"));

            Assert.False(r.Success);
            Assert.NotNull(r.Exception);
            Assert.Contains("UNIQUE", Assert.Single(r.Exception.Errors).ErrorMessage);
            Assert.Single(Db.AllCustomers());
        }

        [Fact]
        public async Task Create_after_failed_insert_on_same_store_succeeds()
        {
            Db.Seed("Alice");
            var failed = await Store.CreateAsync(Dto("Alice"));
            Assert.False(failed.Success);

            var r = await Store.CreateAsync(Dto("Bob"));

            Assert.True(r.Success, r.Exception?.Message);
            Assert.Equal(new[] { "Alice", "Bob" }, Db.AllCustomers().Select(c => c.Name));
        }

        [Fact]
        public async Task Create_with_key_of_already_tracked_entity_returns_failure()
        {
            var a = Dto("Alice");
            await Store.CreateAsync(a);               // entity with a.Id is now tracked
            var r = await Store.CreateAsync(Dto("Bob", id: a.Id));
            Assert.False(r.Success);
            Assert.Single(Db.AllCustomers());
        }

        // ---------------------------------------------------------------- Update / Store

        [Fact]
        public async Task Update_existing_item_persists_changes()
        {
            var seeded = Db.Seed("Alice", score: 1);
            var dto = new CustomerDto { Id = seeded.Id, Name = "Alice2", Email = "a2@test", Score = 5 };

            var r = await Store.UpdateAsync(dto);

            Assert.True(r.Success);
            Assert.Equal(DataMode.Update, r.Mode);
            var row = Assert.Single(Db.AllCustomers());
            Assert.Equal("Alice2", row.Name);
            Assert.Equal(5, row.Score);
            Assert.Equal(DataMode.Update, Assert.Single(Validator.SeenModes));
        }

        [Fact]
        public async Task Update_same_detached_dto_twice_succeeds()
        {
            var seeded = Db.Seed("Alice");
            var dto = new CustomerDto { Id = seeded.Id, Name = "X", Email = "x@test", Score = 1 };
            Assert.True((await Store.UpdateAsync(dto)).Success);

            var dto2 = new CustomerDto { Id = seeded.Id, Name = "Y", Email = "y@test", Score = 2 };
            Assert.True((await Store.UpdateAsync(dto2)).Success);

            Assert.Equal("Y", Assert.Single(Db.AllCustomers()).Name);
        }

        [Fact]
        public async Task Create_then_update_on_same_store_succeeds()
        {
            var dto = Dto("Alice");
            await Store.CreateAsync(dto);
            dto.Score = 10;
            var r = await Store.UpdateAsync(dto);
            Assert.True(r.Success);
            Assert.Equal(10, Assert.Single(Db.AllCustomers()).Score);
        }

        [Fact]
        public async Task Update_non_existent_key_returns_failure()
        {
            var r = await Store.UpdateAsync(Dto("Ghost", id: 999));
            Assert.False(r.Success);
            Assert.Contains("999", Assert.Single(r.Exception.Errors).ErrorMessage);
            Assert.Empty(Db.AllCustomers());
        }

        [Fact]
        public async Task Update_failure_reports_update_mode()
        {
            var r = await Store.UpdateAsync(Dto("Ghost", id: 999));
            Assert.False(r.Success);
            Assert.Equal(DataMode.Update, r.Mode);
        }

        [Fact]
        public async Task Update_with_empty_key_does_not_insert()
        {
            var r = await Store.UpdateAsync(Dto("New"));
            Assert.False(r.Success);
            Assert.Empty(Db.AllCustomers());
        }

        [Fact]
        public async Task Update_invalid_item_returns_errors_and_does_not_persist()
        {
            var seeded = Db.Seed("Alice", score: 1);
            var r = await Store.UpdateAsync(new CustomerDto { Id = seeded.Id, Name = "Alice", Score = -5 });

            Assert.False(r.Success);
            Assert.Contains(r.Exception.Errors, e => e.PropertyName == "Score");
            Assert.Equal(1, Assert.Single(Db.AllCustomers()).Score);
        }

        [Fact]
        public async Task Update_rejected_by_validation_does_not_leak_into_GetByKey()
        {
            var seeded = Db.Seed("Alice", score: 1);
            await Store.UpdateAsync(new CustomerDto { Id = seeded.Id, Name = "Alice", Score = -5 });

            Assert.Equal(1, Store.GetByKey(seeded.Id).Score);
        }

        [Fact]
        public async Task Update_rejected_by_validation_is_not_persisted_by_later_save()
        {
            var seeded = Db.Seed("Alice", score: 1);
            var bad = await Store.UpdateAsync(new CustomerDto { Id = seeded.Id, Name = "Alice", Score = -5 });
            Assert.False(bad.Success);

            Assert.True((await Store.CreateAsync(Dto("Bob"))).Success);

            Assert.Equal(1, Db.AllCustomers().Single(c => c.Name == "Alice").Score);
        }

        [Fact]
        public async Task Store_inserts_new_and_updates_existing()
        {
            var seeded = Db.Seed("Alice", score: 1);
            var r1 = await Store.StoreAsync(Dto("Bob"));
            var r2 = await Store.StoreAsync(new CustomerDto { Id = seeded.Id, Name = "Alice", Email = "a@test", Score = 7 });

            Assert.True(r1.Success);
            Assert.Equal(DataMode.Create, r1.Mode);
            Assert.True(r2.Success);
            Assert.Equal(DataMode.Update, r2.Mode);
            var rows = Db.AllCustomers();
            Assert.Equal(2, rows.Count);
            Assert.Equal(7, rows.Single(c => c.Name == "Alice").Score);
        }

        [Fact]
        public async Task Update_unique_violation_returns_failure()
        {
            Db.Seed("Alice");
            var bob = Db.Seed("Bob");
            var r = await Store.UpdateAsync(new CustomerDto { Id = bob.Id, Name = "Alice", Score = 1 });
            Assert.False(r.Success);
            Assert.Contains("UNIQUE", Assert.Single(r.Exception.Errors).ErrorMessage);
            Assert.Equal(new[] { "Alice", "Bob" }, Db.AllCustomers().Select(c => c.Name));
        }

        // ---------------------------------------------------------------- Delete

        [Fact]
        public async Task Delete_by_key_removes_row()
        {
            var a = Db.Seed("Alice");
            Db.Seed("Bob");
            var r = await Store.DeleteAsync(a.Id);

            Assert.True(r.Success);
            Assert.Equal(DataMode.Delete, r.Mode);
            Assert.Equal("Bob", Assert.Single(Db.AllCustomers()).Name);
            Assert.Equal(DataMode.Delete, Assert.Single(Validator.SeenModes));
        }

        [Fact]
        public async Task Delete_by_model_removes_rows()
        {
            var a = Db.Seed("Alice");
            var b = Db.Seed("Bob");
            var r = await Store.DeleteAsync(new CustomerDto { Id = a.Id }, new CustomerDto { Id = b.Id });
            Assert.True(r.Success);
            Assert.Empty(Db.AllCustomers());
        }

        [Fact]
        public async Task Delete_non_existent_key_is_a_silent_success()
        {
            // Documented current behaviour: missing keys are ignored.
            Db.Seed("Alice");
            var r = await Store.DeleteAsync(12345);
            Assert.True(r.Success);
            Assert.Single(Db.AllCustomers());
        }

        [Fact]
        public async Task Delete_rejected_by_validator_returns_failure_and_rolls_back_batch()
        {
            var a = Db.Seed("Alice");
            var p = Db.Seed("Protected One");
            var r = await Store.DeleteAsync(a.Id, p.Id);

            Assert.False(r.Success);
            Assert.Equal(DataMode.Delete, r.Mode);
            Assert.Contains(r.Exception.Errors, e => e.ErrorMessage.Contains("Protected"));
            Assert.Equal(2, Db.AllCustomers().Count);
        }

        [Fact]
        public async Task Delete_after_rejected_delete_on_same_store_succeeds()
        {
            var a = Db.Seed("Alice");
            var p = Db.Seed("Protected One");
            Assert.False((await Store.DeleteAsync(a.Id, p.Id)).Success);
            Assert.False(Ctx.ChangeTracker.HasChanges());

            Assert.True((await Store.DeleteAsync(a.Id)).Success);
            Assert.Equal("Protected One", Assert.Single(Db.AllCustomers()).Name);
        }

        [Fact]
        public async Task Delete_fk_violation_returns_failed_result()
        {
            var a = Db.Seed("Alice");
            using (var ctx = Db.NewContext())
            {
                ctx.Orders.Add(new Order { CustomerId = a.Id, Product = "P" });
                ctx.SaveChanges();
            }

            var r = await Store.DeleteAsync(a.Id);

            Assert.False(r.Success);
            Assert.Equal(DataMode.Delete, r.Mode);
            Assert.Equal(nameof(Customer), Assert.Single(r.Exception.Errors).PropertyName);
            Assert.Single(Db.AllCustomers());
            Assert.False(Ctx.ChangeTracker.HasChanges());
        }

        // Mapper-specific outcome: declared as facts in the derived classes.
        protected async Task Check_Delete_then_GetByKey_returns_null()
        {
            var a = Db.Seed("Alice");
            await Store.DeleteAsync(a.Id);
            Assert.Null(Store.GetByKey(a.Id));
        }

        // ---------------------------------------------------------------- Read

        [Fact]
        public void GetByKey_returns_mapped_model()
        {
            var a = Db.Seed("Alice", score: 3);
            var dto = Store.GetByKey(a.Id);
            Assert.Equal(a.Id, dto.Id);
            Assert.Equal("Alice", dto.Name);
            Assert.Equal(3, dto.Score);
        }

        protected void Check_GetByKey_missing_key_returns_null()
        {
            Assert.Null(Store.GetByKey(999));
        }

        [Fact]
        public void Query_projects_filters_and_pages()
        {
            for (int i = 0; i < 10; i++) Db.Seed("C" + i, score: i);

            var page = Store.Query().Where(c => c.Score >= 5).OrderBy(c => c.Score).Skip(1).Take(2).ToList();

            Assert.Equal(new[] { 6, 7 }, page.Select(c => c.Score));
            Assert.Equal(new[] { "C6", "C7" }, page.Select(c => c.Name));
        }

        [Fact]
        public void Query_is_translated_to_sql_not_tracked()
        {
            Db.Seed("Alice");
            var sql = Store.Query().Where(c => c.Name == "Alice").ToQueryString();
            Assert.Contains("WHERE", sql);
            Store.Query().ToList();
            Assert.Empty(Ctx.ChangeTracker.Entries());
        }

        protected void Check_Query_honours_custom_mapping_configuration()
        {
            Db.Seed("Alice", email: "a@x");
            var dto = Store.Query().Single();
            Assert.Equal("Alice <a@x>", dto.Display);
        }

        [Fact]
        public void GetByKey_honours_custom_mapping_configuration()
        {
            var a = Db.Seed("Alice", email: "a@x");
            Assert.Equal("Alice <a@x>", Store.GetByKey(a.Id).Display);
        }

        [Fact]
        public void PaginateViaPrimaryKey_defaults_to_false_and_KeyField_is_set()
        {
            Assert.False(Store.PaginateViaPrimaryKey);
            Assert.Equal("Id", Store.KeyField);
        }
    }

    public class MapsterDataStoreTests : DataStoreTestsBase
    {
        protected override IQueryableDataStore<int, CustomerDto> CreateStore(TestDbContext ctx, CustomerValidator validator)
            => new MapsterCustomerStore(ctx, Mappers.Mapster(), validator);

        [Fact]
        public void GetByKey_missing_key_returns_null() => Check_GetByKey_missing_key_returns_null();

        [Fact]
        public Task Delete_then_GetByKey_returns_null() => Check_Delete_then_GetByKey_returns_null();

        [Fact]
        public void Query_honours_custom_mapping_configuration() => Check_Query_honours_custom_mapping_configuration();
    }

    public class AutoMapperDataStoreTests : DataStoreTestsBase
    {
        protected override IQueryableDataStore<int, CustomerDto> CreateStore(TestDbContext ctx, CustomerValidator validator)
            => new AutoMapperCustomerStore(ctx, Mappers.AutoMapper(), validator);

        [Fact]
        public void GetByKey_missing_key_returns_null() => Check_GetByKey_missing_key_returns_null();

        [Fact]
        public Task Delete_then_GetByKey_returns_null() => Check_Delete_then_GetByKey_returns_null();

        [Fact]
        public void Query_honours_custom_mapping_configuration() => Check_Query_honours_custom_mapping_configuration();
    }
}
