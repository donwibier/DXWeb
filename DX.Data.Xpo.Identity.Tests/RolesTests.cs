using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DX.Data.Xpo.Identity.Persistent;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace DX.Data.Xpo.Identity.Tests
{
    /// <summary>
    /// User roles through UserManager and roles through RoleManager, beyond the basics in UserManagerTests
    /// </summary>
    public abstract class RolesTests : IdentityTestBase
    {
        protected RolesTests(MapperKind kind) : base(kind) { }

        [Fact]
        public async Task IsInRole_IgnoresCase()
        {
            await CreateRoleAsync("Administrator");
            var user = await CreateUserAsync();
            await Users.AddToRoleAsync(user, "Administrator");

            Assert.True(await Users.IsInRoleAsync(user, "administrator"));
            Assert.True(await Users.IsInRoleAsync(user, "ADMINISTRATOR"));
            Assert.False(await Users.IsInRoleAsync(user, "Manager"));
        }

        [Fact]
        public async Task AddToRole_InOtherCase_AddsTheRole()
        {
            await CreateRoleAsync("Administrator");
            var user = await CreateUserAsync();

            Assert.True((await Users.AddToRoleAsync(user, "administrator")).Succeeded);

            Assert.Equal(new[] { "Administrator" }, await FreshUsers().GetRolesAsync(user));
        }

        [Fact]
        public async Task AddToRole_Twice_FailsTheSecondTime_AndKeepsOneLink()
        {
            await CreateRoleAsync("Administrator");
            var user = await CreateUserAsync();
            await Users.AddToRoleAsync(user, "Administrator");

            var result = await Users.AddToRoleAsync(user, "Administrator");

            Assert.False(result.Succeeded);
            Assert.Single(await Users.GetRolesAsync(user));
        }

        [Fact(Skip = "BUG: XPBaseUserStore.AddToRoleAsync silently does nothing for an unknown role, so UserManager.AddToRoleAsync reports Success (the Identity contract is InvalidOperationException)")]
        public async Task AddToRole_UnknownRole_Throws()
        {
            var user = await CreateUserAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(() => Users.AddToRoleAsync(user, "Nonexistent"));
        }

        [Fact]
        public async Task AddToRoles_Several_GetRolesSorted()
        {
            await CreateRoleAsync("Waiter");
            await CreateRoleAsync("Cook");
            await CreateRoleAsync("Manager");
            var user = await CreateUserAsync();

            Assert.True((await Users.AddToRolesAsync(user, new[] { "Waiter", "Cook" })).Succeeded);

            Assert.Equal(new[] { "Cook", "Waiter" }, await FreshUsers().GetRolesAsync(user));
        }

        [Fact]
        public async Task GetUsersInRole_ReturnsOnlyItsMembers()
        {
            await CreateRoleAsync("Cook");
            await CreateRoleAsync("Waiter");
            var a = await CreateUserAsync("a@example.com");
            var b = await CreateUserAsync("b@example.com");
            var c = await CreateUserAsync("c@example.com");
            await Users.AddToRoleAsync(b, "Cook");
            await Users.AddToRoleAsync(a, "Cook");
            await Users.AddToRoleAsync(c, "Waiter");

            var cooks = await Users.GetUsersInRoleAsync("cook");

            Assert.Equal(new[] { "a@example.com", "b@example.com" }, cooks.Select(u => u.UserName));
            Assert.Empty(await Users.GetUsersInRoleAsync("Nonexistent"));
        }

        [Fact]
        public async Task RemoveFromRole_LeavesOtherRolesAndOtherUsers()
        {
            await CreateRoleAsync("Cook");
            await CreateRoleAsync("Waiter");
            var user = await CreateUserAsync("one@example.com");
            var other = await CreateUserAsync("two@example.com");
            await Users.AddToRolesAsync(user, new[] { "Cook", "Waiter" });
            await Users.AddToRoleAsync(other, "Cook");

            Assert.True((await Users.RemoveFromRoleAsync(user, "cook")).Succeeded);

            Assert.Equal(new[] { "Waiter" }, await FreshUsers().GetRolesAsync(user));
            Assert.True(await FreshUsers().IsInRoleAsync(other, "Cook"));
        }

        [Fact]
        public async Task RemoveFromRole_ARoleTheUserIsNotIn_Fails()
        {
            await CreateRoleAsync("Cook");
            var user = await CreateUserAsync();

            Assert.False((await Users.RemoveFromRoleAsync(user, "Cook")).Succeeded);
        }

        [Fact]
        public async Task Roles_SurviveAnUpdateOfTheUser()
        {
            await CreateRoleAsync("Cook");
            var user = await CreateUserAsync();
            await Users.AddToRoleAsync(user, "Cook");

            user.PhoneNumber = "+31 6 1234 5678";
            Assert.True((await Users.UpdateAsync(user)).Succeeded);

            Assert.True(await FreshUsers().IsInRoleAsync(user, "Cook"));
        }

        // ---------- RoleManager ----------

        [Fact]
        public async Task Role_CreateDuplicateName_Fails()
        {
            await CreateRoleAsync("Cook");

            var result = await Roles.CreateAsync(new IdentityRole("cook"));

            Assert.False(result.Succeeded);
            Assert.Equal(1, Host.Count<XpoDxRole>());
        }

        [Fact]
        public async Task Role_FindById_AndByNameInOtherCase()
        {
            var role = await CreateRoleAsync("Cook");

            Assert.Equal("Cook", (await Roles.FindByIdAsync(role.Id))?.Name);
            Assert.Equal(role.Id, (await Roles.FindByNameAsync("COOK"))?.Id);
            Assert.Null(await Roles.FindByNameAsync("Waiter"));
        }

        [Fact]
        public async Task Role_Rename_IsFoundByTheNewName_AndKeepsItsUsers()
        {
            var role = await CreateRoleAsync("Cook");
            var user = await CreateUserAsync();
            await Users.AddToRoleAsync(user, "Cook");

            Assert.True((await Roles.SetRoleNameAsync(role, "Chef")).Succeeded);
            Assert.True((await Roles.UpdateAsync(role)).Succeeded);

            Assert.Null(await Roles.FindByNameAsync("Cook"));
            Assert.Equal(role.Id, (await Roles.FindByNameAsync("Chef"))?.Id);
            Assert.True(await FreshUsers().IsInRoleAsync(user, "Chef"));
        }

        [Fact]
        public async Task Role_Delete_RemovesItFromItsUsers()
        {
            var role = await CreateRoleAsync("Cook");
            var user = await CreateUserAsync();
            await Users.AddToRoleAsync(user, "Cook");
            await Roles.AddClaimAsync(role, new Claim("permission", "kitchen"));

            Assert.True((await Roles.DeleteAsync(role)).Succeeded);

            Assert.Null(await Roles.FindByIdAsync(role.Id));
            Assert.Empty(await FreshUsers().GetRolesAsync(user));
            Assert.Equal(0, Host.Count<XpoDxRoleClaim>());
        }

        [Fact]
        public async Task RoleClaims_BelongToTheirRole()
        {
            var cook = await CreateRoleAsync("Cook");
            var waiter = await CreateRoleAsync("Waiter");
            await Roles.AddClaimAsync(cook, new Claim("permission", "kitchen"));
            await Roles.AddClaimAsync(cook, new Claim("permission", "stock"));
            await Roles.AddClaimAsync(waiter, new Claim("permission", "tables"));

            Assert.Equal(new[] { "kitchen", "stock" }, (await Roles.GetClaimsAsync(cook)).Select(c => c.Value).OrderBy(v => v));
            Assert.Equal("tables", Assert.Single(await Roles.GetClaimsAsync(waiter)).Value);
        }

        [Fact]
        public async Task RoleClaims_RemoveOneOfTwo()
        {
            var cook = await CreateRoleAsync("Cook");
            await Roles.AddClaimAsync(cook, new Claim("permission", "kitchen"));
            await Roles.AddClaimAsync(cook, new Claim("permission", "stock"));

            await Roles.RemoveClaimAsync(cook, new Claim("permission", "kitchen"));

            Assert.Equal("stock", Assert.Single(await Roles.GetClaimsAsync(cook)).Value);
        }

        [Fact(Skip = "BUG: XPRoleStore.Roles throws NotImplementedException although the store advertises IQueryableRoleStore (RoleManager.SupportsQueryableRoles is true)")]
        public async Task RoleManager_Roles_CanBeQueried()
        {
            await CreateRoleAsync("Cook");
            await CreateRoleAsync("Waiter");

            Assert.True(Roles.SupportsQueryableRoles);
            Assert.Equal(new[] { "Cook", "Waiter" }, Roles.Roles.Select(r => r.Name).OrderBy(n => n).ToList());
        }

        [Fact]
        public virtual async Task UserManager_Users_CanBeQueried()
        {
            await CreateUserAsync("a@example.com");
            await CreateUserAsync("b@example.com");

            Assert.True(Users.SupportsQueryableUsers);
            Assert.Equal(new[] { "a@example.com", "b@example.com" },
                Users.Users.Where(u => u.UserName.EndsWith("@example.com")).Select(u => u.UserName).OrderBy(n => n).ToList());
        }
    }

    public class RolesTests_Mapster : RolesTests
    {
        public RolesTests_Mapster() : base(MapperKind.Mapster) { }
    }

    public class RolesTests_AutoMapper : RolesTests
    {
        public RolesTests_AutoMapper() : base(MapperKind.AutoMapper) { }

        [Fact(Skip = "BUG (AutoMapper flavour): XPAutoMapperUserStore.Query uses ProjectTo, and XPO cannot translate a Where on the projection, so UserManager.Users.Where(u => u.UserName...) throws NotSupportedException")]
        public override Task UserManager_Users_CanBeQueried() => base.UserManager_Users_CanBeQueried();
    }
}
