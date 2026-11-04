using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using DX.Data.EF.AutoMapper;
using DX.Data.EF.Mapster;
using FluentValidation;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DX.Data.EF.Tests
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int Score { get; set; }
        public List<Order> Orders { get; set; } = new List<Order>();
    }

    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public string Product { get; set; }
    }

    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int Score { get; set; }
        // Only filled by a custom mapping rule (Name + " <" + Email + ">")
        public string Display { get; set; }
    }

    public class TestDbContext : DbContext
    {
        // EFDataStore/EFDatabase require "new()". The parameterless constructor uses this
        // static connection; only EFDatabaseTests rely on it (and they don't run in parallel with each other).
        public static DbConnection DefaultConnection;

        private readonly DbConnection connection;
        private readonly Action<string> log;

        public TestDbContext() : this(DefaultConnection) { }
        public TestDbContext(DbConnection connection, Action<string> log = null)
        {
            this.connection = connection;
            this.log = log;
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(connection);
            if (log != null)
                optionsBuilder.LogTo(log, new[] { DbLoggerCategory.Database.Command.Name });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(e =>
            {
                e.Property(c => c.Name).IsRequired();
                e.HasIndex(c => c.Name).IsUnique();
                e.HasMany(c => c.Orders).WithOne(o => o.Customer)
                    .HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }

    /// <summary>
    /// Validator: Name required, Score >= 0. Customers named "Protected*" may not be deleted.
    /// Records the DataMode / store it saw in the root context data.
    /// </summary>
    public class CustomerValidator : AbstractValidator<Customer>
    {
        public List<DataMode> SeenModes { get; } = new List<DataMode>();
        public List<object> SeenStores { get; } = new List<object>();

        public CustomerValidator()
        {
            RuleFor(c => c).Custom((c, ctx) =>
            {
                var mode = (DataMode)ctx.RootContextData[EFDataStore<TestDbContext, int, CustomerDto, Customer>.CtxMode];
                SeenModes.Add(mode);
                SeenStores.Add(ctx.RootContextData[EFDataStore<TestDbContext, int, CustomerDto, Customer>.CtxStore]);
                if (mode == DataMode.Delete)
                {
                    if (c.Name != null && c.Name.StartsWith("Protected"))
                        ctx.AddFailure("Name", "Protected customers cannot be deleted");
                    return;
                }
                if (string.IsNullOrWhiteSpace(c.Name))
                    ctx.AddFailure("Name", "Name is required");
                if (c.Score < 0)
                    ctx.AddFailure("Score", "Score must be >= 0");
            });
        }
    }

    public class MapsterCustomerStore : EFMapsterDataStore<TestDbContext, int, CustomerDto, Customer>
    {
        public MapsterCustomerStore(TestDbContext context, MapsterMapper.IMapper mapper, IValidator<Customer> validator)
            : base(context, mapper, validator) { }

        public override string KeyField => nameof(CustomerDto.Id);
        public override int ModelKey(CustomerDto model) => model.Id;
        public override void SetModelKey(CustomerDto model, int key) => model.Id = key;
        protected override int DBModelKey(Customer model) => model.Id;

        // expose the protected transactional helper for testing
        public System.Threading.Tasks.Task<T> RunInTransaction<T>(
            Func<TestDbContext, System.Threading.Tasks.Task<T>> work, bool autoCommit = true)
            => TransactionalExecAsync((s, t) => work(s.DbContext), autoCommit);
    }

    public class AutoMapperCustomerStore : EFAutoMapperDataStore<TestDbContext, int, CustomerDto, Customer>
    {
        public AutoMapperCustomerStore(TestDbContext context, global::AutoMapper.IMapper mapper, IValidator<Customer> validator)
            : base(context, mapper, validator) { }

        public override string KeyField => nameof(CustomerDto.Id);
        public override int ModelKey(CustomerDto model) => model.Id;
        public override void SetModelKey(CustomerDto model, int key) => model.Id = key;
        protected override int DBModelKey(Customer model) => model.Id;
    }

    public static class Mappers
    {
        public static global::Mapster.TypeAdapterConfig MapsterConfig()
        {
            var config = new global::Mapster.TypeAdapterConfig();
            config.NewConfig<Customer, CustomerDto>()
                .Map(d => d.Display, s => s.Name + " <" + s.Email + ">");
            config.NewConfig<CustomerDto, Customer>()
                .Ignore(d => d.Orders);
            return config;
        }

        public static MapsterMapper.IMapper Mapster() => new MapsterMapper.Mapper(MapsterConfig());

        public static global::AutoMapper.IMapper AutoMapper()
        {
            var config = new global::AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Customer, CustomerDto>()
                    .ForMember(d => d.Display, o => o.MapFrom(s => s.Name + " <" + s.Email + ">"));
                cfg.CreateMap<CustomerDto, Customer>()
                    .ForMember(d => d.Orders, o => o.Ignore());
            }, NullLoggerFactory.Instance);
            return config.CreateMapper();
        }
    }

    /// <summary>Per-test SQLite in-memory database with a kept-open connection.</summary>
    public sealed class SqliteDb : IDisposable
    {
        public SqliteConnection Connection { get; }
        public List<string> Log { get; } = new List<string>();

        public SqliteDb()
        {
            Connection = new SqliteConnection("Data Source=:memory:");
            Connection.Open();
            using (var ctx = new TestDbContext(Connection))
                ctx.Database.EnsureCreated();
        }

        public TestDbContext NewContext(bool logSql = false)
            => new TestDbContext(Connection, logSql ? (Action<string>)(s => { lock (Log) Log.Add(s); }) : null);

        public List<Customer> AllCustomers()
        {
            using (var ctx = NewContext())
                return ctx.Customers.AsNoTracking().OrderBy(c => c.Id).ToList();
        }

        public Customer Seed(string name, int score = 1, string email = null)
        {
            using (var ctx = NewContext())
            {
                var c = new Customer { Name = name, Score = score, Email = email ?? name.ToLower() + "@test" };
                ctx.Customers.Add(c);
                ctx.SaveChanges();
                return c;
            }
        }

        public void Dispose() => Connection.Dispose();
    }
}
