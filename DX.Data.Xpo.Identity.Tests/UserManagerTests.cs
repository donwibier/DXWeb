using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DX.Data.Xpo.Identity.Mapster;
using DX.Data.Xpo.Identity.Persistent;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DX.Data.Xpo.Identity.Tests
{
    /// <summary>
    /// The identity stores as an application registers them (AddXpoMapsterIdentityStores), through UserManager and
    /// RoleManager, on an in-memory XpoDatabase
    /// </summary>
    public class UserManagerTests : IDisposable
    {
        private readonly ServiceProvider _services;
        private readonly IServiceScope _scope;

        public UserManagerTests()
        {
            // XpoDatabase keeps its data layers by name for the whole process: a name per test gets a fresh database
            var name = "Identity" + Guid.NewGuid().ToString("N");
            var services = new ServiceCollection();
            services.AddLogging();
            // as in an application; the stores ask for it, although the connection comes from XpoDatabase here
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            services.AddSingleton(new XpoDatabase(o =>
            {
                o.Name = name;
                o.ConnectionString = InMemoryDataStore.GetConnectionStringInMemory(true);
                o.UpdateSchema = AutoCreateOption.DatabaseAndSchema;
            }));
            services.AddIdentityCore<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddXpoMapsterIdentityStores(name);
            _services = services.BuildServiceProvider();
            _scope = _services.CreateScope();
        }

        public void Dispose()
        {
            _scope.Dispose();
            _services.Dispose();
        }

        private UserManager<IdentityUser> Users => _scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        private RoleManager<IdentityRole> Roles => _scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        private async Task<IdentityUser> CreateUserAsync(string email = "user@example.com")
        {
            var result = await Users.CreateAsync(new IdentityUser { UserName = email, Email = email }, "Secret-123");
            Assert.True(result.Succeeded, string.Join("; ", result.Errors));
            return await Users.FindByEmailAsync(email);
        }

        [Fact]
        public async Task Create_ThenFindByEmailAndName()
        {
            var user = await CreateUserAsync();

            Assert.NotNull(user);
            Assert.Equal(user.Id, (await Users.FindByNameAsync("user@example.com"))?.Id);
            Assert.Equal(user.Id, (await Users.FindByIdAsync(user.Id))?.Id);
        }

        [Fact]
        public async Task Password_IsChecked()
        {
            var user = await CreateUserAsync();

            Assert.True(await Users.CheckPasswordAsync(user, "Secret-123"));
            Assert.False(await Users.CheckPasswordAsync(user, "wrong"));
        }

        [Fact]
        public async Task Roles_AddCheckAndRemove()
        {
            Assert.True((await Roles.CreateAsync(new IdentityRole("Administrator"))).Succeeded);
            var user = await CreateUserAsync();

            await Users.AddToRoleAsync(user, "Administrator");
            Assert.True(await Users.IsInRoleAsync(user, "Administrator"));
            Assert.Contains("Administrator", await Users.GetRolesAsync(user));

            await Users.RemoveFromRoleAsync(user, "Administrator");
            Assert.False(await Users.IsInRoleAsync(user, "Administrator"));
        }

        [Fact]
        public async Task Claims_AddAndGet()
        {
            var user = await CreateUserAsync();

            await Users.AddClaimAsync(user, new Claim("department", "kitchen"));

            var claim = Assert.Single(await Users.GetClaimsAsync(user));
            Assert.Equal("department", claim.Type);
            Assert.Equal("kitchen", claim.Value);
        }

        [Fact]
        public async Task Delete_UserWithRoleAndClaim_IsDeleted_AndTheRoleStays()
        {
            Assert.True((await Roles.CreateAsync(new IdentityRole("Administrator"))).Succeeded);
            var user = await CreateUserAsync();
            await Users.AddToRoleAsync(user, "Administrator");
            await Users.AddClaimAsync(user, new Claim("department", "kitchen"));

            var result = await Users.DeleteAsync(user);

            Assert.True(result.Succeeded, string.Join("; ", result.Errors));
            Assert.Null(await Users.FindByIdAsync(user.Id));
            Assert.NotNull(await Roles.FindByNameAsync("Administrator"));
            Assert.Empty(await Users.GetUsersInRoleAsync("Administrator"));
        }

        [Fact]
        public async Task Claims_Remove_KeepsTheOthers()
        {
            var user = await CreateUserAsync();
            await Users.AddClaimAsync(user, new Claim("department", "kitchen"));
            await Users.AddClaimAsync(user, new Claim("shift", "evening"));

            await Users.RemoveClaimAsync(user, new Claim("department", "kitchen"));

            var claim = Assert.Single(await Users.GetClaimsAsync(user));
            Assert.Equal("shift", claim.Type);
        }

        [Fact]
        public async Task Claims_Replace()
        {
            var user = await CreateUserAsync();
            await Users.AddClaimAsync(user, new Claim("department", "kitchen"));

            await Users.ReplaceClaimAsync(user, new Claim("department", "kitchen"), new Claim("department", "delivery"));

            Assert.Equal("delivery", Assert.Single(await Users.GetClaimsAsync(user)).Value);
        }

        [Fact]
        public async Task Logins_AddFindAndRemove()
        {
            var user = await CreateUserAsync();

            await Users.AddLoginAsync(user, new UserLoginInfo("Google", "google-key", "Google"));
            Assert.Equal(user.Id, (await Users.FindByLoginAsync("Google", "google-key"))?.Id);
            Assert.Single(await Users.GetLoginsAsync(user));

            await Users.RemoveLoginAsync(user, "Google", "google-key");
            Assert.Empty(await Users.GetLoginsAsync(user));
        }

        [Fact]
        public async Task RoleClaims_AddAndRemove()
        {
            Assert.True((await Roles.CreateAsync(new IdentityRole("Manager"))).Succeeded);
            var role = await Roles.FindByNameAsync("Manager");

            await Roles.AddClaimAsync(role, new Claim("permission", "menu"));
            Assert.Equal("menu", Assert.Single(await Roles.GetClaimsAsync(role)).Value);

            await Roles.RemoveClaimAsync(role, new Claim("permission", "menu"));
            Assert.Empty(await Roles.GetClaimsAsync(role));
        }
    }
}
