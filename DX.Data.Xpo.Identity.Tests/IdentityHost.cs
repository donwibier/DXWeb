using System;
using System.Threading.Tasks;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DX.Data.Xpo.Identity.AutoMapper;
using DX.Data.Xpo.Identity.Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DX.Data.Xpo.Identity.Tests
{
    public enum MapperKind { Mapster, AutoMapper }

    /// <summary>
    /// A service provider as an application builds it (AddIdentityCore + the XPO stores of one mapper flavour) on a
    /// fresh in-memory XpoDatabase, with the TOTP token providers that need no Data Protection
    /// </summary>
    public sealed class IdentityHost : IDisposable
    {
        private readonly ServiceProvider _services;

        public IdentityHost(MapperKind kind, Action<IdentityOptions> configure = null)
        {
            // XpoDatabase keeps its data layers by name for the whole process: a name per host gets a fresh database
            var name = "Identity" + Guid.NewGuid().ToString("N");
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            services.AddSingleton(new XpoDatabase(o =>
            {
                o.Name = name;
                o.ConnectionString = InMemoryDataStore.GetConnectionStringInMemory(true);
                o.UpdateSchema = AutoCreateOption.DatabaseAndSchema;
            }));
            var builder = services.AddIdentityCore<IdentityUser>(o =>
                {
                    o.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
                    o.Tokens.ChangeEmailTokenProvider = TokenOptions.DefaultEmailProvider;
                    o.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
                    o.Tokens.ChangePhoneNumberTokenProvider = TokenOptions.DefaultPhoneProvider;
                    configure?.Invoke(o);
                })
                .AddRoles<IdentityRole>()
                .AddTokenProvider<EmailTokenProvider<IdentityUser>>(TokenOptions.DefaultEmailProvider)
                .AddTokenProvider<PhoneNumberTokenProvider<IdentityUser>>(TokenOptions.DefaultPhoneProvider)
                .AddTokenProvider<AuthenticatorTokenProvider<IdentityUser>>(TokenOptions.DefaultAuthenticatorProvider);
            if (kind == MapperKind.Mapster)
                builder.AddXpoMapsterIdentityStores(name);
            else
                builder.AddXpoAutoMapperIdentityStores(name);
            _services = services.BuildServiceProvider();
            Kind = kind;
        }

        public MapperKind Kind { get; }

        public IServiceProvider Root => _services;

        public IServiceScope NewScope() => _services.CreateScope();

        public IDataLayer DataLayer => _services.GetRequiredService<IDataLayer>();

        public int Count<T>() where T : class
        {
            using var uow = new UnitOfWork(DataLayer);
            return new XPCollection<T>(uow).Count;
        }

        public void Dispose() => _services.Dispose();
    }

    /// <summary>
    /// Base for tests that run against both store flavours: one scope per test, as a request in an application
    /// </summary>
    public abstract class IdentityTestBase : IDisposable
    {
        protected IdentityTestBase(MapperKind kind, Action<IdentityOptions> configure = null)
        {
            Host = new IdentityHost(kind, configure);
            Scope = Host.NewScope();
        }

        protected IdentityHost Host { get; }
        protected IServiceScope Scope { get; }

        protected UserManager<IdentityUser> Users => Scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        protected RoleManager<IdentityRole> Roles => Scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        /// <summary>The user manager of a new scope, as the next request would have it</summary>
        protected UserManager<IdentityUser> FreshUsers() => NewScope().ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        private readonly System.Collections.Generic.List<IServiceScope> _scopes = new();

        /// <summary>A new scope, disposed with the test</summary>
        protected IServiceScope NewScope()
        {
            var scope = Host.NewScope();
            _scopes.Add(scope);
            return scope;
        }

        protected async Task<IdentityUser> CreateUserAsync(string email = "user@example.com", string password = "Secret-123")
        {
            var user = new IdentityUser { UserName = email, Email = email };
            var result = password == null ? await Users.CreateAsync(user) : await Users.CreateAsync(user, password);
            Assert.True(result.Succeeded, string.Join("; ", result.Errors));
            return await Users.FindByIdAsync(user.Id);
        }

        protected async Task<IdentityRole> CreateRoleAsync(string name)
        {
            var result = await Roles.CreateAsync(new IdentityRole(name));
            Assert.True(result.Succeeded, string.Join("; ", result.Errors));
            return await Roles.FindByNameAsync(name);
        }

        /// <summary>The user as the next request loads it from the database</summary>
        protected Task<IdentityUser> ReloadAsync(IdentityUser user) => FreshUsers().FindByIdAsync(user.Id);

        public virtual void Dispose()
        {
            foreach (var s in _scopes)
                s.Dispose();
            Scope.Dispose();
            Host.Dispose();
        }
    }
}
