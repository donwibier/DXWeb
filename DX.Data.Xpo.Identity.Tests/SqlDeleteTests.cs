using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DX.Data.Xpo.Identity.Persistent;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DX.Data.Xpo.Identity.Tests
{
    /// <summary>
    /// Deleting users and roles on a SQL database (SQLite file), where the stores delete in a database level transaction
    /// and remove the role links ahead of the object, so XPO does not load every member of a role
    /// </summary>
    public abstract class SqlDeleteTests : IDisposable
    {
        private readonly string _file = Path.Combine(Path.GetTempPath(), "dxidentity-" + Guid.NewGuid().ToString("N") + ".db");
        private readonly IdentityHost _host;

        protected SqlDeleteTests(MapperKind kind)
        {
            _host = new IdentityHost(kind, connectionString: "XpoProvider=SQLite;Data Source=" + _file);
        }

        public void Dispose()
        {
            _host.Dispose();
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            try { File.Delete(_file); } catch (IOException) { }
        }

        private async Task<T> InScope<T>(Func<UserManager<IdentityUser>, RoleManager<IdentityRole>, Task<T>> work)
        {
            using var scope = _host.NewScope();
            return await work(scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>(),
                scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>());
        }

        private async Task SeedAsync(int members)
        {
            await InScope(async (users, roles) =>
            {
                Assert.True((await roles.CreateAsync(new IdentityRole("Members"))).Succeeded);
                Assert.True((await roles.CreateAsync(new IdentityRole("Admins"))).Succeeded);
                for (int i = 0; i < members; i++)
                {
                    var user = new IdentityUser("user" + i) { Email = $"user{i}@example.com" };
                    Assert.True((await users.CreateAsync(user)).Succeeded);
                    Assert.True((await users.AddToRoleAsync(user, "Members")).Succeeded);
                    if (i == 0)
                        Assert.True((await users.AddToRoleAsync(user, "Admins")).Succeeded);
                }
                return 0;
            });
        }

        [Fact]
        public async Task DeleteUser_RemovesItsRoleLinks_AndKeepsTheOtherMembers()
        {
            await SeedAsync(20);

            var deleted = await InScope(async (users, roles) => await users.DeleteAsync(await users.FindByNameAsync("user0")));

            Assert.True(deleted.Succeeded, string.Join(", ", deleted.Errors.Select(e => e.Description)));
            await InScope(async (users, roles) =>
            {
                Assert.Null(await users.FindByNameAsync("user0"));
                Assert.Equal(19, (await users.GetUsersInRoleAsync("Members")).Count);
                Assert.Empty(await users.GetUsersInRoleAsync("Admins"));
                Assert.Equal(new[] { "Members" }, await users.GetRolesAsync(await users.FindByNameAsync("user1")));
                return 0;
            });
            Assert.Equal(19, _host.Count<XpoDxUser>());
        }

        [Fact]
        public async Task DeleteRole_RemovesItsLinks_AndKeepsTheUsers()
        {
            await SeedAsync(20);

            var deleted = await InScope(async (users, roles) => await roles.DeleteAsync(await roles.FindByNameAsync("Members")));

            Assert.True(deleted.Succeeded, string.Join(", ", deleted.Errors.Select(e => e.Description)));
            await InScope(async (users, roles) =>
            {
                Assert.Null(await roles.FindByNameAsync("Members"));
                Assert.Equal(new[] { "Admins" }, await users.GetRolesAsync(await users.FindByNameAsync("user0")));
                Assert.Empty(await users.GetRolesAsync(await users.FindByNameAsync("user1")));
                return 0;
            });
            Assert.Equal(20, _host.Count<XpoDxUser>());
            Assert.Equal(1, _host.Count<XpoDxRole>());
        }

        [Fact]
        public async Task DeleteUser_WithClaimsLoginsAndTokens_Cascades()
        {
            await SeedAsync(3);
            await InScope(async (users, roles) =>
            {
                var user = await users.FindByNameAsync("user0");
                Assert.True((await users.AddClaimAsync(user, new System.Security.Claims.Claim("t", "v"))).Succeeded);
                Assert.True((await users.AddLoginAsync(user, new UserLoginInfo("p", "k", "P"))).Succeeded);
                Assert.True((await users.SetAuthenticationTokenAsync(user, "p", "n", "v")).Succeeded);
                return 0;
            });

            var deleted = await InScope(async (users, roles) => await users.DeleteAsync(await users.FindByNameAsync("user0")));

            Assert.True(deleted.Succeeded, string.Join(", ", deleted.Errors.Select(e => e.Description)));
            Assert.Equal(0, _host.Count<XpoDxUserClaim>());
            Assert.Equal(0, _host.Count<XpoDxUserLogin>());
            Assert.Equal(0, _host.Count<XpoDxUserToken>());
            Assert.Equal(2, _host.Count<XpoDxUser>());
        }
    }

    public class SqlDeleteTests_Mapster : SqlDeleteTests { public SqlDeleteTests_Mapster() : base(MapperKind.Mapster) { } }
    public class SqlDeleteTests_AutoMapper : SqlDeleteTests { public SqlDeleteTests_AutoMapper() : base(MapperKind.AutoMapper) { } }
}
