using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DX.Data.Xpo.Identity.Persistent;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace DX.Data.Xpo.Identity.Tests
{
    /// <summary>
    /// User claims, external logins and authentication tokens through UserManager, beyond the basics in UserManagerTests
    /// </summary>
    public abstract class ClaimsLoginsTokensTests : IdentityTestBase
    {
        protected ClaimsLoginsTokensTests(MapperKind kind) : base(kind) { }

        // ---------- claims ----------

        [Fact]
        public async Task Claims_AddSeveralInOneCall_AllArePersisted()
        {
            var user = await CreateUserAsync();

            await Users.AddClaimsAsync(user, new[] { new Claim("a", "1"), new Claim("b", "2"), new Claim("c", "3") });

            var claims = await FreshUsers().GetClaimsAsync(user);
            Assert.Equal(new[] { "a", "b", "c" }, claims.Select(c => c.Type).OrderBy(t => t));
        }

        [Fact]
        public async Task Claims_SameTypeDifferentValues_AreKeptApart()
        {
            var user = await CreateUserAsync();

            await Users.AddClaimAsync(user, new Claim("role", "cook"));
            await Users.AddClaimAsync(user, new Claim("role", "waiter"));

            var values = (await Users.GetClaimsAsync(user)).Select(c => c.Value).OrderBy(v => v);
            Assert.Equal(new[] { "cook", "waiter" }, values);
        }

        [Fact]
        public async Task Claims_AddingTheSameClaimTwice_DoesNotThrow_AndKeepsIt()
        {
            var user = await CreateUserAsync();

            await Users.AddClaimAsync(user, new Claim("department", "kitchen"));
            await Users.AddClaimAsync(user, new Claim("department", "kitchen"));

            Assert.Contains(await Users.GetClaimsAsync(user), c => c.Type == "department" && c.Value == "kitchen");
        }

        [Fact]
        public async Task Claims_RemoveSeveral_LeavesTheRest()
        {
            var user = await CreateUserAsync();
            await Users.AddClaimsAsync(user, new[] { new Claim("a", "1"), new Claim("b", "2"), new Claim("c", "3") });

            await Users.RemoveClaimsAsync(user, new[] { new Claim("a", "1"), new Claim("c", "3") });

            Assert.Equal("b", Assert.Single(await Users.GetClaimsAsync(user)).Type);
            Assert.Equal(1, Host.Count<XpoDxUserClaim>());
        }

        [Fact]
        public async Task Claims_Remove_OnlyAffectsThatUser()
        {
            var user = await CreateUserAsync("one@example.com");
            var other = await CreateUserAsync("two@example.com");
            await Users.AddClaimAsync(user, new Claim("department", "kitchen"));
            await Users.AddClaimAsync(other, new Claim("department", "kitchen"));

            await Users.RemoveClaimAsync(user, new Claim("department", "kitchen"));

            Assert.Empty(await Users.GetClaimsAsync(user));
            Assert.Single(await Users.GetClaimsAsync(other));
        }

        [Fact]
        public async Task Claims_Replace_OnlyAffectsThatUser()
        {
            var user = await CreateUserAsync("one@example.com");
            var other = await CreateUserAsync("two@example.com");
            await Users.AddClaimAsync(user, new Claim("department", "kitchen"));
            await Users.AddClaimAsync(other, new Claim("department", "kitchen"));

            await Users.ReplaceClaimAsync(user, new Claim("department", "kitchen"), new Claim("department", "bar"));

            Assert.Equal("bar", Assert.Single(await Users.GetClaimsAsync(user)).Value);
            Assert.Equal("kitchen", Assert.Single(await Users.GetClaimsAsync(other)).Value);
        }

        [Fact]
        public async Task Claims_ReplaceAClaimTheUserDoesNotHave_ChangesNothing()
        {
            var user = await CreateUserAsync();
            await Users.AddClaimAsync(user, new Claim("department", "kitchen"));

            await Users.ReplaceClaimAsync(user, new Claim("department", "garden"), new Claim("department", "bar"));

            Assert.Equal("kitchen", Assert.Single(await Users.GetClaimsAsync(user)).Value);
        }

        [Fact]
        public async Task Claims_SurviveAnUpdateOfTheUser()
        {
            var user = await CreateUserAsync();
            await Users.AddClaimAsync(user, new Claim("department", "kitchen"));

            user.PhoneNumber = "+31 6 1234 5678";
            Assert.True((await Users.UpdateAsync(user)).Succeeded);

            Assert.Single(await FreshUsers().GetClaimsAsync(user));
        }

        [Fact]
        public async Task Claims_GetUsersForClaim_ReturnsTheUsersWithIt()
        {
            var user = await CreateUserAsync("one@example.com");
            var other = await CreateUserAsync("two@example.com");
            await CreateUserAsync("three@example.com");
            await Users.AddClaimAsync(user, new Claim("department", "kitchen"));
            await Users.AddClaimAsync(other, new Claim("department", "kitchen"));

            var users = await Users.GetUsersForClaimAsync(new Claim("department", "kitchen"));

            Assert.Equal(new[] { "one@example.com", "two@example.com" }, users.Select(u => u.UserName).OrderBy(n => n));
        }

        // ---------- logins ----------

        [Fact]
        public async Task Logins_FindByUnknownProviderKey_ReturnsNull()
        {
            var user = await CreateUserAsync();
            await Users.AddLoginAsync(user, new UserLoginInfo("Google", "google-key", "Google"));

            Assert.Null(await Users.FindByLoginAsync("Google", "other-key"));
            Assert.Null(await Users.FindByLoginAsync("Microsoft", "google-key"));
        }

        [Fact]
        public async Task Logins_FindByProviderKey_PicksTheRightUser()
        {
            var user = await CreateUserAsync("one@example.com");
            var other = await CreateUserAsync("two@example.com");
            await Users.AddLoginAsync(user, new UserLoginInfo("Google", "key-1", "Google"));
            await Users.AddLoginAsync(other, new UserLoginInfo("Google", "key-2", "Google"));

            Assert.Equal(other.Id, (await Users.FindByLoginAsync("Google", "key-2"))?.Id);
            Assert.Equal(user.Id, (await Users.FindByLoginAsync("Google", "key-1"))?.Id);
        }

        [Fact]
        public async Task Logins_TwoProvidersForOneUser()
        {
            var user = await CreateUserAsync();

            await Users.AddLoginAsync(user, new UserLoginInfo("Google", "google-key", "Google"));
            await Users.AddLoginAsync(user, new UserLoginInfo("Microsoft", "ms-key", "Microsoft"));

            Assert.Equal(new[] { "Google", "Microsoft" }, (await FreshUsers().GetLoginsAsync(user)).Select(l => l.LoginProvider).OrderBy(p => p));
        }

        [Fact]
        public async Task Logins_KeepTheProviderDisplayName()
        {
            var user = await CreateUserAsync();
            await Users.AddLoginAsync(user, new UserLoginInfo("Google", "google-key", "Google account"));

            var login = Assert.Single(await FreshUsers().GetLoginsAsync(user));
            Assert.Equal("Google", login.LoginProvider);
            Assert.Equal("google-key", login.ProviderKey);
            Assert.Equal("Google account", login.ProviderDisplayName);
        }

        [Fact]
        public async Task Logins_AddingTheSameLoginToASecondUser_Fails()
        {
            var user = await CreateUserAsync("one@example.com");
            var other = await CreateUserAsync("two@example.com");
            await Users.AddLoginAsync(user, new UserLoginInfo("Google", "google-key", "Google"));

            // UserManager checks FindByLoginAsync first and reports LoginAlreadyAssociated
            var result = await Users.AddLoginAsync(other, new UserLoginInfo("Google", "google-key", "Google"));

            Assert.False(result.Succeeded);
            Assert.Empty(await Users.GetLoginsAsync(other));
        }

        [Fact]
        public async Task Logins_RemoveAnotherUsersLogin_ChangesNothing()
        {
            var user = await CreateUserAsync("one@example.com");
            var other = await CreateUserAsync("two@example.com");
            await Users.AddLoginAsync(user, new UserLoginInfo("Google", "google-key", "Google"));

            await Users.RemoveLoginAsync(other, "Google", "google-key");

            Assert.Single(await Users.GetLoginsAsync(user));
        }

        // ---------- tokens ----------

        [Fact]
        public virtual async Task Tokens_SetGetAndRemove()
        {
            var user = await CreateUserAsync();

            Assert.True((await Users.SetAuthenticationTokenAsync(user, "Google", "access_token", "abc")).Succeeded);
            Assert.Equal("abc", await FreshUsers().GetAuthenticationTokenAsync(user, "Google", "access_token"));

            Assert.True((await Users.RemoveAuthenticationTokenAsync(user, "Google", "access_token")).Succeeded);
            Assert.Null(await FreshUsers().GetAuthenticationTokenAsync(user, "Google", "access_token"));
            Assert.Equal(0, Host.Count<XpoDxUserToken>());
        }

        [Fact]
        public virtual async Task Tokens_AreKeptPerProviderAndName()
        {
            var user = await CreateUserAsync();

            await Users.SetAuthenticationTokenAsync(user, "Google", "access_token", "google-access");
            await Users.SetAuthenticationTokenAsync(user, "Google", "refresh_token", "google-refresh");
            await Users.SetAuthenticationTokenAsync(user, "Microsoft", "access_token", "ms-access");

            var users = FreshUsers();
            Assert.Equal("google-access", await users.GetAuthenticationTokenAsync(user, "Google", "access_token"));
            Assert.Equal("google-refresh", await users.GetAuthenticationTokenAsync(user, "Google", "refresh_token"));
            Assert.Equal("ms-access", await users.GetAuthenticationTokenAsync(user, "Microsoft", "access_token"));
        }

        [Fact(Skip = "BUG: SetTokenAsync on an existing token only changes the DTO from FindTokenAsync; XPUserStore never writes it back, so the old value stays")]
        public async Task Tokens_SetTwice_KeepsTheNewValue()
        {
            var user = await CreateUserAsync();

            await Users.SetAuthenticationTokenAsync(user, "Google", "access_token", "first");
            await Users.SetAuthenticationTokenAsync(user, "Google", "access_token", "second");

            Assert.Equal("second", await FreshUsers().GetAuthenticationTokenAsync(user, "Google", "access_token"));
        }

        [Fact]
        public virtual async Task Authenticator_ResetKey_StoresAKey()
        {
            var user = await CreateUserAsync();

            Assert.True((await Users.ResetAuthenticatorKeyAsync(user)).Succeeded);

            Assert.False(string.IsNullOrEmpty(await FreshUsers().GetAuthenticatorKeyAsync(user)));
        }

        [Fact(Skip = "BUG: same as Tokens_SetTwice: a second ResetAuthenticatorKeyAsync keeps the old key, so a leaked authenticator key cannot be rotated")]
        public async Task Authenticator_ResetKeyTwice_GivesANewKey()
        {
            var user = await CreateUserAsync();
            await Users.ResetAuthenticatorKeyAsync(user);
            var first = await FreshUsers().GetAuthenticatorKeyAsync(user);

            await Users.ResetAuthenticatorKeyAsync(user);

            Assert.NotEqual(first, await FreshUsers().GetAuthenticatorKeyAsync(user));
        }

        [Fact]
        public virtual async Task RecoveryCodes_GenerateAndCount()
        {
            var user = await CreateUserAsync();

            var codes = (await Users.GenerateNewTwoFactorRecoveryCodesAsync(user, 10)).ToList();

            Assert.Equal(10, codes.Count);
            Assert.Equal(10, await FreshUsers().CountRecoveryCodesAsync(user));
        }

        [Fact]
        public async Task RecoveryCodes_RedeemAnUnknownCode_Fails()
        {
            var user = await CreateUserAsync();
            await Users.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

            Assert.False((await Users.RedeemTwoFactorRecoveryCodeAsync(user, "not-a-code")).Succeeded);
        }

        [Fact(Skip = "BUG: RedeemCodeAsync rewrites the RecoveryCodes token via SetTokenAsync, which XPUserStore never persists: a redeemed recovery code stays valid and can be reused")]
        public async Task RecoveryCodes_ARedeemedCode_CannotBeUsedAgain()
        {
            var user = await CreateUserAsync();
            var code = (await Users.GenerateNewTwoFactorRecoveryCodesAsync(user, 10)).First();

            Assert.True((await Users.RedeemTwoFactorRecoveryCodeAsync(user, code)).Succeeded);

            var next = FreshUsers();
            var reloaded = await next.FindByIdAsync(user.Id);
            Assert.Equal(9, await next.CountRecoveryCodesAsync(reloaded));
            Assert.False((await next.RedeemTwoFactorRecoveryCodeAsync(reloaded, code)).Succeeded);
        }

        [Fact(Skip = "BUG: same as Tokens_SetTwice: generating new recovery codes keeps the old set, so the new codes do not work and the old ones still do")]
        public async Task RecoveryCodes_GenerateAgain_ReplacesTheOldSet()
        {
            var user = await CreateUserAsync();
            var old = (await Users.GenerateNewTwoFactorRecoveryCodesAsync(user, 10)).First();
            var fresh = (await Users.GenerateNewTwoFactorRecoveryCodesAsync(user, 10)).First();

            var next = FreshUsers();
            var reloaded = await next.FindByIdAsync(user.Id);
            Assert.False((await next.RedeemTwoFactorRecoveryCodeAsync(reloaded, old)).Succeeded);
            Assert.True((await next.RedeemTwoFactorRecoveryCodeAsync(reloaded, fresh)).Succeeded);
        }
    }

    public class ClaimsLoginsTokensTests_Mapster : ClaimsLoginsTokensTests
    {
        public ClaimsLoginsTokensTests_Mapster() : base(MapperKind.Mapster) { }
    }

    public class ClaimsLoginsTokensTests_AutoMapper : ClaimsLoginsTokensTests
    {
        public const string TokenNotStoredBug = "BUG (AutoMapper flavour): the XPIdentityMapperProfile maps IdentityUserToken.UserId onto the get-only XpoDxUserToken.UserId alias, so User stays null, validation fails, and XPUserStore.AddUserTokenAsync ignores the failed result: SetAuthenticationTokenAsync reports Success and stores nothing";

        public ClaimsLoginsTokensTests_AutoMapper() : base(MapperKind.AutoMapper) { }

        [Fact(Skip = TokenNotStoredBug)]
        public override Task Tokens_SetGetAndRemove() => base.Tokens_SetGetAndRemove();

        [Fact(Skip = TokenNotStoredBug)]
        public override Task Tokens_AreKeptPerProviderAndName() => base.Tokens_AreKeptPerProviderAndName();

        [Fact(Skip = TokenNotStoredBug)]
        public override Task Authenticator_ResetKey_StoresAKey() => base.Authenticator_ResetKey_StoresAKey();

        [Fact(Skip = TokenNotStoredBug)]
        public override Task RecoveryCodes_GenerateAndCount() => base.RecoveryCodes_GenerateAndCount();
    }
}
