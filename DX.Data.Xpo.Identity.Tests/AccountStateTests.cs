using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DX.Data.Xpo.Identity.Persistent;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DX.Data.Xpo.Identity.Tests
{
    /// <summary>
    /// Lockout, stamps, email/phone confirmation, passwords, lookups, delete and detached updates through UserManager;
    /// every check reloads the user in a new scope, so it sees what was persisted rather than the object in memory
    /// </summary>
    public abstract class AccountStateTests : IdentityTestBase
    {
        protected AccountStateTests(MapperKind kind) : base(kind) { }

        // ---------- lockout and access failed ----------

        [Fact(Skip = "BUG: IdentityUser.LockoutEnd is never mapped to XpoDxUser.LockoutEndDateUtc (names differ), so a lockout is lost on reload")]
        public async Task Lockout_SetEndDate_IsPersisted()
        {
            var user = await CreateUserAsync();
            var end = DateTimeOffset.UtcNow.AddHours(1);

            Assert.True((await Users.SetLockoutEndDateAsync(user, end)).Succeeded);

            var reloaded = await ReloadAsync(user);
            Assert.NotNull(reloaded.LockoutEnd);
            Assert.True(await FreshUsers().IsLockedOutAsync(reloaded));
        }

        [Fact(Skip = "BUG: as Lockout_SetEndDate_IsPersisted: after MaxFailedAccessAttempts the lockout end is not persisted, so brute-force lockout never takes effect")]
        public async Task Lockout_AfterMaxFailedAttempts_TheUserIsLockedOut()
        {
            var user = await CreateUserAsync();
            for (var i = 0; i < 5; i++)
                await FreshUsers().AccessFailedAsync(await ReloadAsync(user));

            Assert.True(await FreshUsers().IsLockedOutAsync(await ReloadAsync(user)));
        }

        [Fact]
        public async Task AccessFailed_IncrementsAndResets_AndIsPersisted()
        {
            var user = await CreateUserAsync();

            await Users.AccessFailedAsync(user);
            await Users.AccessFailedAsync(user);
            Assert.Equal(2, (await ReloadAsync(user)).AccessFailedCount);

            await Users.ResetAccessFailedCountAsync(user);
            Assert.Equal(0, (await ReloadAsync(user)).AccessFailedCount);
        }

        [Fact]
        public async Task LockoutEnabled_IsPersisted()
        {
            var user = await CreateUserAsync();
            Assert.True(user.LockoutEnabled);

            await Users.SetLockoutEnabledAsync(user, false);

            Assert.False((await ReloadAsync(user)).LockoutEnabled);
        }

        // ---------- stamps ----------

        [Fact]
        public async Task Create_SetsSecurityAndConcurrencyStamps()
        {
            var user = await CreateUserAsync();

            Assert.False(string.IsNullOrEmpty(user.SecurityStamp));
            Assert.False(string.IsNullOrEmpty(user.ConcurrencyStamp));
        }

        [Fact]
        public async Task UpdateSecurityStamp_ChangesAndPersistsIt()
        {
            var user = await CreateUserAsync();
            var before = user.SecurityStamp;

            Assert.True((await Users.UpdateSecurityStampAsync(user)).Succeeded);

            var after = (await ReloadAsync(user)).SecurityStamp;
            Assert.NotEqual(before, after);
            Assert.Equal(user.SecurityStamp, after);
            Assert.Equal(after, await FreshUsers().GetSecurityStampAsync(user));
        }

        [Fact]
        public async Task Update_ChangesAndPersistsTheConcurrencyStamp()
        {
            var user = await CreateUserAsync();
            var before = user.ConcurrencyStamp;

            user.PhoneNumber = "123";
            Assert.True((await Users.UpdateAsync(user)).Succeeded);

            Assert.NotEqual(before, user.ConcurrencyStamp);
            Assert.Equal(user.ConcurrencyStamp, (await ReloadAsync(user)).ConcurrencyStamp);
        }

        [Fact(Skip = "BUG: XPUserStore.UpdateAsync never compares the ConcurrencyStamp, so a stale copy overwrites a newer one instead of failing with ConcurrencyFailure")]
        public async Task Update_OfAStaleCopy_FailsWithConcurrencyFailure()
        {
            await CreateUserAsync();
            var first = FreshUsers();
            var second = FreshUsers();
            var copy1 = await first.FindByNameAsync("user@example.com");
            var copy2 = await second.FindByNameAsync("user@example.com");

            copy1.PhoneNumber = "111";
            Assert.True((await first.UpdateAsync(copy1)).Succeeded);
            copy2.PhoneNumber = "222";
            var result = await second.UpdateAsync(copy2);

            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, e => e.Code == nameof(IdentityErrorDescriber.ConcurrencyFailure));
            Assert.Equal("111", (await ReloadAsync(copy1)).PhoneNumber);
        }

        // ---------- email and phone ----------

        [Fact]
        public async Task ConfirmEmail_WithAGeneratedToken_IsPersisted()
        {
            var user = await CreateUserAsync();
            Assert.False(user.EmailConfirmed);

            var token = await Users.GenerateEmailConfirmationTokenAsync(user);
            Assert.True((await FreshUsers().ConfirmEmailAsync(await ReloadAsync(user), token)).Succeeded);

            Assert.True((await ReloadAsync(user)).EmailConfirmed);
        }

        [Fact]
        public async Task ConfirmEmail_WithAWrongToken_Fails()
        {
            var user = await CreateUserAsync();

            Assert.False((await Users.ConfirmEmailAsync(user, "000000")).Succeeded);
            Assert.False((await ReloadAsync(user)).EmailConfirmed);
        }

        [Fact]
        public async Task SetEmail_IsFoundByTheNewEmail_AndIsUnconfirmed()
        {
            var user = await CreateUserAsync();
            var token = await Users.GenerateEmailConfirmationTokenAsync(user);
            await Users.ConfirmEmailAsync(user, token);

            Assert.True((await Users.SetEmailAsync(user, "new@example.com")).Succeeded);

            var users = FreshUsers();
            Assert.Null(await users.FindByEmailAsync("user@example.com"));
            var found = await users.FindByEmailAsync("NEW@example.com");
            Assert.Equal(user.Id, found?.Id);
            Assert.False(found.EmailConfirmed);
        }

        [Fact]
        public async Task ChangePhoneNumber_WithAGeneratedToken_SetsAndConfirmsIt()
        {
            var user = await CreateUserAsync();

            var token = await Users.GenerateChangePhoneNumberTokenAsync(user, "+31612345678");
            Assert.True((await Users.ChangePhoneNumberAsync(user, "+31612345678", token)).Succeeded);

            var reloaded = await ReloadAsync(user);
            Assert.Equal("+31612345678", reloaded.PhoneNumber);
            Assert.True(reloaded.PhoneNumberConfirmed);
        }

        [Fact]
        public async Task SetPhoneNumberAndTwoFactor_ArePersisted()
        {
            var user = await CreateUserAsync();

            await Users.SetPhoneNumberAsync(user, "+31612345678");
            await Users.SetTwoFactorEnabledAsync(user, true);

            var reloaded = await ReloadAsync(user);
            Assert.Equal("+31612345678", reloaded.PhoneNumber);
            Assert.False(reloaded.PhoneNumberConfirmed);
            Assert.True(reloaded.TwoFactorEnabled);
        }

        // ---------- passwords ----------

        [Fact]
        public async Task ChangePassword_OldFails_NewWorks()
        {
            var user = await CreateUserAsync();

            Assert.True((await Users.ChangePasswordAsync(user, "Secret-123", "Other-456")).Succeeded);

            var users = FreshUsers();
            var reloaded = await users.FindByIdAsync(user.Id);
            Assert.False(await users.CheckPasswordAsync(reloaded, "Secret-123"));
            Assert.True(await users.CheckPasswordAsync(reloaded, "Other-456"));
        }

        [Fact]
        public async Task ChangePassword_WithTheWrongCurrentPassword_Fails()
        {
            var user = await CreateUserAsync();

            Assert.False((await Users.ChangePasswordAsync(user, "wrong", "Other-456")).Succeeded);
            Assert.True(await FreshUsers().CheckPasswordAsync(await ReloadAsync(user), "Secret-123"));
        }

        [Fact]
        public async Task RemoveAndAddPassword()
        {
            var user = await CreateUserAsync();

            Assert.True((await Users.RemovePasswordAsync(user)).Succeeded);
            Assert.False(await FreshUsers().HasPasswordAsync(await ReloadAsync(user)));

            var reloaded = await ReloadAsync(user);
            var users = FreshUsers();
            Assert.True((await users.AddPasswordAsync(reloaded, "Other-456")).Succeeded);
            Assert.True(await FreshUsers().CheckPasswordAsync(await ReloadAsync(user), "Other-456"));
        }

        [Fact]
        public async Task ResetPassword_WithAGeneratedToken()
        {
            var user = await CreateUserAsync();

            var token = await Users.GeneratePasswordResetTokenAsync(user);
            Assert.True((await FreshUsers().ResetPasswordAsync(await ReloadAsync(user), token, "Other-456")).Succeeded);

            Assert.True(await FreshUsers().CheckPasswordAsync(await ReloadAsync(user), "Other-456"));
        }

        [Fact]
        public async Task Create_WithoutPassword_HasNoPassword()
        {
            var user = await CreateUserAsync(password: null);

            Assert.False(await Users.HasPasswordAsync(user));
            Assert.False(await Users.CheckPasswordAsync(user, "anything"));
        }

        // ---------- lookups ----------

        [Fact]
        public async Task FindByName_AndByEmail_IgnoreCase()
        {
            var user = await CreateUserAsync("Mixed.Case@Example.com");

            Assert.Equal(user.Id, (await Users.FindByNameAsync("mixed.case@example.com"))?.Id);
            Assert.Equal(user.Id, (await Users.FindByEmailAsync("MIXED.CASE@EXAMPLE.COM"))?.Id);
        }

        [Fact]
        public async Task FindById_Unknown_ReturnsNull()
        {
            await CreateUserAsync();

            Assert.Null(await Users.FindByIdAsync(Guid.NewGuid().ToString()));
            Assert.Null(await Users.FindByNameAsync("nobody@example.com"));
            Assert.Null(await Users.FindByEmailAsync("nobody@example.com"));
        }

        [Fact]
        public async Task Create_DuplicateUserName_Fails()
        {
            await CreateUserAsync();

            var result = await Users.CreateAsync(new IdentityUser { UserName = "USER@example.com", Email = "x@example.com" }, "Secret-123");

            Assert.False(result.Succeeded);
            Assert.Equal(1, Host.Count<XpoDxUser>());
        }

        [Fact]
        public async Task FindByEmail_WithTwoUsersSharingIt_ReturnsOneOfThem()
        {
            // RequireUniqueEmail is false by default, so two users can share an address. The EF store throws here
            // (SingleOrDefault); this store returns the first match, which the test pins down as its behaviour.
            var a = await CreateUserAsync("a@example.com");
            var b = new IdentityUser { UserName = "b@example.com", Email = "a@example.com" };
            Assert.True((await Users.CreateAsync(b, "Secret-123")).Succeeded);

            var found = await Users.FindByEmailAsync("a@example.com");

            Assert.Contains(found?.Id, new[] { a.Id, b.Id });
        }

        // ---------- delete ----------

        [Fact]
        public virtual async Task Delete_CascadesClaimsLoginsTokensAndRoleLinks_ButNotOtherUsers()
        {
            await CreateRoleAsync("Cook");
            var user = await CreateUserAsync("one@example.com");
            var other = await CreateUserAsync("two@example.com");
            foreach (var u in new[] { user, other })
            {
                await Users.AddToRoleAsync(u, "Cook");
                await Users.AddClaimAsync(u, new Claim("department", "kitchen"));
                await Users.SetAuthenticationTokenAsync(u, "Google", "access_token", "token-" + u.UserName);
            }
            // one login only: a second login in the table trips the AddLoginAsync bug (ClaimsLoginsTokensTests.LoginEqualsBug)
            await Users.AddLoginAsync(user, new UserLoginInfo("Google", "key-one", "Google"));

            Assert.True((await Users.DeleteAsync(user)).Succeeded);

            Assert.Null(await Users.FindByLoginAsync("Google", "key-one"));
            Assert.Equal(1, Host.Count<XpoDxUser>());
            Assert.Equal(1, Host.Count<XpoDxUserClaim>());
            Assert.Equal(0, Host.Count<XpoDxUserLogin>());
            Assert.Equal(1, Host.Count<XpoDxUserToken>());
            Assert.Equal(new[] { "two@example.com" }, (await Users.GetUsersInRoleAsync("Cook")).Select(u => u.UserName));
            Assert.Equal("token-two@example.com", await Users.GetAuthenticationTokenAsync(other, "Google", "access_token"));
        }

        [Fact]
        public async Task Delete_AnAlreadyDeletedUser_DoesNotThrow()
        {
            var user = await CreateUserAsync();
            await Users.DeleteAsync(user);

            var result = await FreshUsers().DeleteAsync(user);

            Assert.NotNull(result);
        }

        // ---------- detached update ----------

        [Fact]
        public async Task Update_OfAUserLoadedInAnEarlierScope_IsPersisted_AndKeepsTheRest()
        {
            await CreateRoleAsync("Cook");
            var created = await CreateUserAsync();
            await Users.AddToRoleAsync(created, "Cook");
            await Users.AddClaimAsync(created, new Claim("department", "kitchen"));

            IdentityUser detached;
            using (var earlier = Host.NewScope())
                detached = await earlier.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>().FindByIdAsync(created.Id);

            using (var later = Host.NewScope())
            {
                var users = later.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                detached.PhoneNumber = "+31612345678";
                Assert.True((await users.UpdateAsync(detached)).Succeeded);
            }

            var users2 = FreshUsers();
            var reloaded = await users2.FindByIdAsync(created.Id);
            Assert.Equal("+31612345678", reloaded.PhoneNumber);
            Assert.True(await users2.CheckPasswordAsync(reloaded, "Secret-123"));
            Assert.True(await users2.IsInRoleAsync(reloaded, "Cook"));
            Assert.Single(await users2.GetClaimsAsync(reloaded));
        }

        [Fact]
        public async Task SetUserName_IsFoundByTheNewName()
        {
            var user = await CreateUserAsync();

            Assert.True((await Users.SetUserNameAsync(user, "renamed@example.com")).Succeeded);

            var users = FreshUsers();
            Assert.Null(await users.FindByNameAsync("user@example.com"));
            Assert.Equal(user.Id, (await users.FindByNameAsync("renamed@example.com"))?.Id);
        }
    }

    public class AccountStateTests_Mapster : AccountStateTests
    {
        public AccountStateTests_Mapster() : base(MapperKind.Mapster) { }
    }

    public class AccountStateTests_AutoMapper : AccountStateTests
    {
        public AccountStateTests_AutoMapper() : base(MapperKind.AutoMapper) { }

        [Fact(Skip = ClaimsLoginsTokensTests_AutoMapper.TokenNotStoredBug)]
        public override Task Delete_CascadesClaimsLoginsTokensAndRoleLinks_ButNotOtherUsers() => base.Delete_CascadesClaimsLoginsTokensAndRoleLinks_ButNotOtherUsers();
    }
}
