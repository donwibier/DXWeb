using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DX.Data.Xpo.Identity.Tests
{
    /// <summary>
    /// The IUserStore / IRoleStore contract the stores are resolved as, called directly: a cancelled token throws
    /// OperationCanceledException, a null argument ArgumentNullException (not a NullReferenceException further down)
    /// </summary>
    public class StoreContractTests : IdentityTestBase
    {
        private const string NotForwarded = "BUG: XPUserStore does not pass the CancellationToken on to the data store (FindById/Name/Email, GetClaims, GetLogins), so a cancelled request still queries";
        private const string NullDeref = "BUG: no null check: user.Id / role.Id is dereferenced, so null throws NullReferenceException instead of ArgumentNullException";

        public StoreContractTests() : base(MapperKind.Mapster) { }

        private IUserStore<IdentityUser> UserStore => Scope.ServiceProvider.GetRequiredService<IUserStore<IdentityUser>>();
        private IRoleStore<IdentityRole> RoleStore => Scope.ServiceProvider.GetRequiredService<IRoleStore<IdentityRole>>();

        private Dictionary<string, Func<IdentityUser, IdentityRole, CancellationToken, Task>> Calls()
        {
            var us = UserStore;
            var rs = RoleStore;
            var claim = new Claim("t", "v");
            return new Dictionary<string, Func<IdentityUser, IdentityRole, CancellationToken, Task>>
            {
                ["User.Create"] = (u, r, ct) => us.CreateAsync(u ?? new IdentityUser("x@example.com") { Email = "x@example.com" }, ct),
                ["User.Update"] = (u, r, ct) => us.UpdateAsync(u, ct),
                ["User.Delete"] = (u, r, ct) => us.DeleteAsync(u, ct),
                ["User.FindById"] = (u, r, ct) => us.FindByIdAsync(u?.Id ?? Guid.NewGuid().ToString(), ct),
                ["User.FindByName"] = (u, r, ct) => us.FindByNameAsync("USER@EXAMPLE.COM", ct),
                ["User.FindByEmail"] = (u, r, ct) => ((IUserEmailStore<IdentityUser>)us).FindByEmailAsync("USER@EXAMPLE.COM", ct),
                ["User.GetClaims"] = (u, r, ct) => ((IUserClaimStore<IdentityUser>)us).GetClaimsAsync(u, ct),
                ["User.AddClaims"] = (u, r, ct) => ((IUserClaimStore<IdentityUser>)us).AddClaimsAsync(u, new[] { claim }, ct),
                ["User.ReplaceClaim"] = (u, r, ct) => ((IUserClaimStore<IdentityUser>)us).ReplaceClaimAsync(u, claim, claim, ct),
                ["User.RemoveClaims"] = (u, r, ct) => ((IUserClaimStore<IdentityUser>)us).RemoveClaimsAsync(u, new[] { claim }, ct),
                ["User.GetUsersForClaim"] = (u, r, ct) => ((IUserClaimStore<IdentityUser>)us).GetUsersForClaimAsync(claim, ct),
                ["User.GetLogins"] = (u, r, ct) => ((IUserLoginStore<IdentityUser>)us).GetLoginsAsync(u, ct),
                ["User.AddLogin"] = (u, r, ct) => ((IUserLoginStore<IdentityUser>)us).AddLoginAsync(u, new UserLoginInfo("p", "k", "d"), ct),
                ["User.RemoveLogin"] = (u, r, ct) => ((IUserLoginStore<IdentityUser>)us).RemoveLoginAsync(u, "p", "k", ct),
                ["User.FindByLogin"] = (u, r, ct) => ((IUserLoginStore<IdentityUser>)us).FindByLoginAsync("p", "k", ct),
                ["User.AddToRole"] = (u, r, ct) => ((IUserRoleStore<IdentityUser>)us).AddToRoleAsync(u, "COOK", ct),
                ["User.RemoveFromRole"] = (u, r, ct) => ((IUserRoleStore<IdentityUser>)us).RemoveFromRoleAsync(u, "COOK", ct),
                ["User.GetRoles"] = (u, r, ct) => ((IUserRoleStore<IdentityUser>)us).GetRolesAsync(u, ct),
                ["User.IsInRole"] = (u, r, ct) => ((IUserRoleStore<IdentityUser>)us).IsInRoleAsync(u, "COOK", ct),
                ["User.GetUsersInRole"] = (u, r, ct) => ((IUserRoleStore<IdentityUser>)us).GetUsersInRoleAsync("COOK", ct),
                ["User.SetToken"] = (u, r, ct) => ((IUserAuthenticationTokenStore<IdentityUser>)us).SetTokenAsync(u, "p", "n", "v", ct),
                ["User.GetToken"] = (u, r, ct) => ((IUserAuthenticationTokenStore<IdentityUser>)us).GetTokenAsync(u, "p", "n", ct),
                ["User.RemoveToken"] = (u, r, ct) => ((IUserAuthenticationTokenStore<IdentityUser>)us).RemoveTokenAsync(u, "p", "n", ct),
                ["User.SetPasswordHash"] = (u, r, ct) => ((IUserPasswordStore<IdentityUser>)us).SetPasswordHashAsync(u, "hash", ct),
                ["User.SetSecurityStamp"] = (u, r, ct) => ((IUserSecurityStampStore<IdentityUser>)us).SetSecurityStampAsync(u, "stamp", ct),
                ["User.RedeemCode"] = (u, r, ct) => ((IUserTwoFactorRecoveryCodeStore<IdentityUser>)us).RedeemCodeAsync(u, "code", ct),
                ["Role.Create"] = (u, r, ct) => rs.CreateAsync(r ?? new IdentityRole("Waiter"), ct),
                ["Role.Update"] = (u, r, ct) => rs.UpdateAsync(r, ct),
                ["Role.Delete"] = (u, r, ct) => rs.DeleteAsync(r, ct),
                ["Role.FindById"] = (u, r, ct) => rs.FindByIdAsync(r?.Id ?? Guid.NewGuid().ToString(), ct),
                ["Role.FindByName"] = (u, r, ct) => rs.FindByNameAsync("COOK", ct),
                ["Role.GetClaims"] = (u, r, ct) => ((IRoleClaimStore<IdentityRole>)rs).GetClaimsAsync(r, ct),
                ["Role.AddClaim"] = (u, r, ct) => ((IRoleClaimStore<IdentityRole>)rs).AddClaimAsync(r, new Claim("t", "v"), ct),
                ["Role.RemoveClaim"] = (u, r, ct) => ((IRoleClaimStore<IdentityRole>)rs).RemoveClaimAsync(r, new Claim("t", "v"), ct),
            };
        }

        [Theory]
        [InlineData("User.Create")]
        [InlineData("User.Update")]
        [InlineData("User.Delete")]
        [InlineData("User.FindById", Skip = NotForwarded)]
        [InlineData("User.FindByName", Skip = NotForwarded)]
        [InlineData("User.FindByEmail", Skip = NotForwarded)]
        [InlineData("User.GetClaims", Skip = NotForwarded)]
        [InlineData("User.AddClaims")]
        [InlineData("User.ReplaceClaim")]
        [InlineData("User.RemoveClaims")]
        [InlineData("User.GetUsersForClaim")]
        [InlineData("User.GetLogins", Skip = NotForwarded)]
        [InlineData("User.AddLogin")]
        [InlineData("User.RemoveLogin")]
        [InlineData("User.FindByLogin")]
        [InlineData("User.AddToRole")]
        [InlineData("User.RemoveFromRole")]
        [InlineData("User.GetRoles")]
        [InlineData("User.IsInRole")]
        [InlineData("User.GetUsersInRole")]
        [InlineData("User.SetToken")]
        [InlineData("User.GetToken")]
        [InlineData("User.RemoveToken")]
        [InlineData("User.SetPasswordHash")]
        [InlineData("User.SetSecurityStamp")]
        [InlineData("User.RedeemCode")]
        [InlineData("Role.Create")]
        [InlineData("Role.Update")]
        [InlineData("Role.Delete")]
        [InlineData("Role.FindById")]
        [InlineData("Role.FindByName")]
        [InlineData("Role.GetClaims")]
        [InlineData("Role.AddClaim")]
        [InlineData("Role.RemoveClaim")]
        public async Task CancelledToken_Throws(string call)
        {
            var user = await CreateUserAsync();
            var role = await CreateRoleAsync("Cook");

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Calls()[call](user, role, new CancellationToken(true)));
        }

        [Theory]
        [InlineData("User.Create")]
        [InlineData("User.Update")]
        [InlineData("User.Delete")]
        [InlineData("User.GetClaims", Skip = NullDeref)]
        [InlineData("User.AddClaims")]
        [InlineData("User.ReplaceClaim")]
        [InlineData("User.RemoveClaims")]
        [InlineData("User.GetLogins", Skip = NullDeref)]
        [InlineData("User.AddLogin")]
        [InlineData("User.RemoveLogin")]
        [InlineData("User.AddToRole")]
        [InlineData("User.RemoveFromRole")]
        [InlineData("User.GetRoles")]
        [InlineData("User.IsInRole")]
        [InlineData("User.SetToken")]
        [InlineData("User.GetToken")]
        [InlineData("User.RemoveToken")]
        [InlineData("User.SetPasswordHash")]
        [InlineData("User.SetSecurityStamp")]
        [InlineData("User.RedeemCode")]
        [InlineData("Role.Create")]
        [InlineData("Role.Update")]
        [InlineData("Role.Delete")]
        [InlineData("Role.GetClaims", Skip = NullDeref)]
        [InlineData("Role.AddClaim")]
        [InlineData("Role.RemoveClaim")]
        public async Task NullUserOrRole_ThrowsArgumentNullException(string call)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => NullCall(call));
        }

        // Create with a null user: the dictionary substitutes a user for null, so call these directly
        private Task NullCall(string call) => call switch
        {
            "User.Create" => UserStore.CreateAsync(null, default),
            "Role.Create" => RoleStore.CreateAsync(null, default),
            _ => Calls()[call](null, null, default),
        };

        [Theory]
        [InlineData("User.FindById")]
        [InlineData("User.FindByName")]
        [InlineData("User.FindByEmail")]
        public async Task NullKey_ThrowsArgumentNullException(string call)
        {
            var us = UserStore;
            Task t = call switch
            {
                "User.FindById" => us.FindByIdAsync(null, default),
                "User.FindByName" => us.FindByNameAsync(null, default),
                _ => ((IUserEmailStore<IdentityUser>)us).FindByEmailAsync(null, default),
            };
            await Assert.ThrowsAsync<ArgumentNullException>(() => t);
        }

        [Fact]
        public async Task NullClaimLoginOrRoleName_ThrowsArgumentNullException()
        {
            var user = await CreateUserAsync();
            var us = UserStore;

            await Assert.ThrowsAsync<ArgumentNullException>(() => ((IUserClaimStore<IdentityUser>)us).AddClaimsAsync(user, null, default));
            await Assert.ThrowsAsync<ArgumentNullException>(() => ((IUserClaimStore<IdentityUser>)us).RemoveClaimsAsync(user, null, default));
            await Assert.ThrowsAsync<ArgumentNullException>(() => ((IUserClaimStore<IdentityUser>)us).GetUsersForClaimAsync(null, default));
            await Assert.ThrowsAsync<ArgumentNullException>(() => ((IUserLoginStore<IdentityUser>)us).AddLoginAsync(user, null, default));
            await Assert.ThrowsAsync<ArgumentNullException>(() => ((IUserRoleStore<IdentityUser>)us).AddToRoleAsync(user, null, default));
            await Assert.ThrowsAsync<ArgumentNullException>(() => ((IUserRoleStore<IdentityUser>)us).GetUsersInRoleAsync(null, default));
        }

        private sealed class DerivedUserStore : XPUserStore<string, IdentityUser, IdentityRole, IdentityUserClaim<string>, IdentityUserRole<string>,
            IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>,
            Persistent.XpoDxUser, Persistent.XpoDxRole, Persistent.XpoDxUserLogin, Persistent.XpoDxUserClaim, Persistent.XpoDxUserToken>
        {
            public DerivedUserStore(IServiceProvider sp) : base(
                sp.GetRequiredService<IQueryableUserStore<string, IdentityUser, IdentityUserRole<string>, IdentityUserToken<string>>>(),
                sp.GetRequiredService<IQueryableRoleStore<string, IdentityRole>>(),
                sp.GetRequiredService<IQueryableUserClaimStore<string, IdentityUserClaim<string>>>(),
                sp.GetRequiredService<IQueryableUserLoginStore<string, IdentityUserLogin<string>>>(),
                sp.GetRequiredService<IQueryableUserTokenStore<string, IdentityUserToken<string>>>())
            { }

            public Task<IdentityUserRole<string>> CallFindUserRoleAsync(string userId, string roleId) => FindUserRoleAsync(userId, roleId, default);
        }

        // Unskipped this does not fail, it kills the test host: verified in a scratch console app ("Stack overflow. Repeated 14141 times")
        [Fact(Skip = "BUG: XPUserStore.FindUserRoleAsync calls itself instead of _UserStore.FindUserRoleAsync: infinite recursion, StackOverflow (crashes the test host)")]
        public async Task FindUserRole_ForAUserInTheRole_ReturnsTheLink()
        {
            var user = await CreateUserAsync();
            var role = await CreateRoleAsync("Cook");
            await Users.AddToRoleAsync(user, "Cook");

            var link = await new DerivedUserStore(Scope.ServiceProvider).CallFindUserRoleAsync(user.Id, role.Id);

            Assert.Equal(user.Id, link?.UserId);
            Assert.Equal(role.Id, link?.RoleId);
        }

        [Fact]
        public async Task Store_AfterDispose_ThrowsObjectDisposedException()
        {
            var user = await CreateUserAsync();
            var us = UserStore;
            us.Dispose();

            await Assert.ThrowsAsync<ObjectDisposedException>(() => us.UpdateAsync(user, default));
        }
    }
}
