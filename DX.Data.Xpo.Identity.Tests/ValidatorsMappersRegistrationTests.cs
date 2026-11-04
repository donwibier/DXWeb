using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Xpo;
using DX.Data.Xpo.Identity.Persistent;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

#pragma warning disable CS0618 // XPUserMapper / XPRoleMapper are obsolete, still public API

namespace DX.Data.Xpo.Identity.Tests
{
    public class ValidatorTests
    {
        private readonly InMemoryDb _db = new InMemoryDb();

        [Fact]
        public void User_Valid()
        {
            using var uow = _db.NewUnitOfWork();
            Assert.True(new XPIdentityUserValidator().Validate(InMemoryDb.NewUser(uow, "user@example.com")).IsValid);
        }

        [Theory]
        [InlineData("not-an-email", "user")]
        [InlineData("", "user")]
        [InlineData(null, "user")]
        [InlineData("user@example.com", "")]
        [InlineData("user@example.com", null)]
        public void User_InvalidEmailOrEmptyUserName_IsRejected(string email, string userName)
        {
            using var uow = _db.NewUnitOfWork();
            var user = new XpoDxUser(uow) { Email = email, UserName = userName };

            Assert.False(new XPIdentityUserValidator().Validate(user).IsValid);
        }

        [Fact]
        public void UserClaim_NeedsAUserAndAType()
        {
            using var uow = _db.NewUnitOfWork();
            var user = InMemoryDb.NewUser(uow, "user@example.com");
            uow.CommitChanges(); // XpoDxBase gives the Id on saving
            var v = new XPIdentityUserClaimValidator();

            Assert.True(v.Validate(new XpoDxUserClaim(uow) { User = user, ClaimType = "t", ClaimValue = "" }).IsValid);
            Assert.False(v.Validate(new XpoDxUserClaim(uow) { User = user, ClaimType = "", ClaimValue = "v" }).IsValid);
            Assert.False(v.Validate(new XpoDxUserClaim(uow) { ClaimType = "t", ClaimValue = "v" }).IsValid);
        }

        [Fact]
        public void UserLogin_NeedsAUserProviderAndKey()
        {
            using var uow = _db.NewUnitOfWork();
            var user = InMemoryDb.NewUser(uow, "user@example.com");
            uow.CommitChanges(); // XpoDxBase gives the Id on saving
            var v = new XPIdentityUserLoginValidator();

            Assert.True(v.Validate(new XpoDxUserLogin(uow) { User = user, LoginProvider = "p", ProviderKey = "k" }).IsValid);
            Assert.False(v.Validate(new XpoDxUserLogin(uow) { User = user, LoginProvider = "", ProviderKey = "k" }).IsValid);
            Assert.False(v.Validate(new XpoDxUserLogin(uow) { User = user, LoginProvider = "p", ProviderKey = "" }).IsValid);
            Assert.False(v.Validate(new XpoDxUserLogin(uow) { LoginProvider = "p", ProviderKey = "k" }).IsValid);
        }

        [Fact]
        public void UserToken_NeedsAUserNameAndValue()
        {
            using var uow = _db.NewUnitOfWork();
            var user = InMemoryDb.NewUser(uow, "user@example.com");
            uow.CommitChanges(); // XpoDxBase gives the Id on saving
            var v = new XPIdentityUserTokenValidator();

            Assert.True(v.Validate(new XpoDxUserToken(uow) { User = user, LoginProvider = "p", Name = "n", Value = "v" }).IsValid);
            Assert.False(v.Validate(new XpoDxUserToken(uow) { User = user, LoginProvider = "p", Name = "", Value = "v" }).IsValid);
            Assert.False(v.Validate(new XpoDxUserToken(uow) { User = user, LoginProvider = "p", Name = "n", Value = "" }).IsValid);
        }

        [Fact]
        public void Role_NeedsAName()
        {
            using var uow = _db.NewUnitOfWork();
            var v = new XPIdentityRoleValidator();

            Assert.True(v.Validate(InMemoryDb.NewRole(uow, "Cook")).IsValid);
            Assert.False(v.Validate(new XpoDxRole(uow)).IsValid);
        }

        [Fact]
        public void RoleClaim_NeedsARoleTypeAndValue()
        {
            using var uow = _db.NewUnitOfWork();
            var role = InMemoryDb.NewRole(uow, "Cook");
            uow.CommitChanges();
            var v = new XPIdentityRoleClaimValidator();

            Assert.True(v.Validate(new XpoDxRoleClaim(uow) { Role = role, ClaimType = "t", ClaimValue = "v" }).IsValid);
            Assert.False(v.Validate(new XpoDxRoleClaim(uow) { Role = role, ClaimType = "", ClaimValue = "v" }).IsValid);
            Assert.False(v.Validate(new XpoDxRoleClaim(uow) { ClaimType = "t", ClaimValue = "v" }).IsValid);
        }
    }

    /// <summary>The validator is what CreateAsync runs: a user it rejects is not stored</summary>
    public class ValidatorThroughUserManagerTests : IdentityTestBase
    {
        public ValidatorThroughUserManagerTests() : base(MapperKind.Mapster) { }

        [Fact]
        public async Task Create_WithAnInvalidEmail_Fails_AndStoresNothing()
        {
            var result = await Users.CreateAsync(new IdentityUser { UserName = "user", Email = "not-an-email" }, "Secret-123");

            Assert.False(result.Succeeded);
            Assert.Equal(0, Host.Count<XpoDxUser>());
        }
    }

    public class LegacyMapperTests
    {
        public class PlainUser : IXPUser<string>
        {
            public string Id { get; set; }
            public string UserName { get; set; }
            public string NormalizedUserName { get; set; }
            public string NormalizedEmail { get; set; }
            public string Email { get; set; }
            public bool EmailConfirmed { get; set; }
            public string PasswordHash { get; set; }
            public string SecurityStamp { get; set; }
            public string PhoneNumber { get; set; }
            public bool PhoneNumberConfirmed { get; set; }
            public bool TwoFactorEnabled { get; set; }
            public bool LockoutEnabled { get; set; }
            public DateTime? LockoutEndDateUtc { get; set; }
            public int AccessFailedCount { get; set; }
            public string ConcurrencyStamp { get; set; }
            public string RefreshToken { get; set; }
            public DateTime? RefreshTokenExpiryTime { get; set; }
            public IList RolesList { get; } = new List<object>();
            public IList ClaimsList { get; } = new List<object>();
            public IList LoginsList { get; } = new List<object>();
            public IList TokenList { get; } = new List<object>();
        }

        public class PlainRole : IXPRole<string>
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string NormalizedName { get; set; }
            public string ConcurrencyStamp { get; set; }
            public IList ClaimsList { get; } = new List<object>();
        }

        private readonly InMemoryDb _db = new InMemoryDb();

        private static PlainUser Sample() => new PlainUser
        {
            Id = "id-1",
            UserName = "user@example.com",
            NormalizedUserName = "USER@EXAMPLE.COM",
            Email = "user@example.com",
            NormalizedEmail = "USER@EXAMPLE.COM",
            EmailConfirmed = true,
            PasswordHash = "hash",
            SecurityStamp = "stamp",
            PhoneNumber = "123",
            PhoneNumberConfirmed = true,
            TwoFactorEnabled = true,
            LockoutEnabled = true,
            LockoutEndDateUtc = new DateTime(2030, 1, 2, 3, 4, 5, DateTimeKind.Utc),
            AccessFailedCount = 3,
            ConcurrencyStamp = "concurrency",
            RefreshToken = "refresh",
            RefreshTokenExpiryTime = new DateTime(2031, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };

        [Fact]
        public void UserMapper_RoundTrip_KeepsTheIdentityFields()
        {
            var mapper = new XPUserMapper<PlainUser, XpoDxUser>();
            using var uow = _db.NewUnitOfWork();
            var source = Sample();

            var db = mapper.Assign(source, new XpoDxUser(uow));
            var back = mapper.CreateModel(db);

            Assert.Equal(source.UserName, back.UserName);
            Assert.Equal(source.NormalizedUserName, back.NormalizedUserName);
            Assert.Equal(source.Email, back.Email);
            Assert.Equal(source.NormalizedEmail, back.NormalizedEmail);
            Assert.Equal(source.EmailConfirmed, back.EmailConfirmed);
            Assert.Equal(source.PasswordHash, back.PasswordHash);
            Assert.Equal(source.SecurityStamp, back.SecurityStamp);
            Assert.Equal(source.PhoneNumber, back.PhoneNumber);
            Assert.Equal(source.PhoneNumberConfirmed, back.PhoneNumberConfirmed);
            Assert.Equal(source.TwoFactorEnabled, back.TwoFactorEnabled);
            Assert.Equal(source.LockoutEnabled, back.LockoutEnabled);
            Assert.Equal(source.LockoutEndDateUtc, back.LockoutEndDateUtc);
            Assert.Equal(source.AccessFailedCount, back.AccessFailedCount);
            Assert.Equal(source.RefreshToken, back.RefreshToken);
            Assert.Equal(source.RefreshTokenExpiryTime, back.RefreshTokenExpiryTime);
        }

        [Fact]
        public void UserMapper_RoundTrip_KeepsTheConcurrencyStamp()
        {
            var mapper = new XPUserMapper<PlainUser, XpoDxUser>();
            using var uow = _db.NewUnitOfWork();

            var db = mapper.Assign(Sample(), new XpoDxUser(uow));
            Assert.Equal("concurrency", db.ConcurrencyStamp);
            Assert.Equal("concurrency", mapper.CreateModel(db).ConcurrencyStamp);
        }

        [Fact]
        public void UserMapper_Assign_DoesNotOverwriteTheKey()
        {
            var mapper = new XPUserMapper<PlainUser, XpoDxUser>();
            using var uow = _db.NewUnitOfWork();
            var db = InMemoryDb.NewUser(uow, "old@example.com");
            uow.CommitChanges();
            var id = db.Id;
            Assert.False(string.IsNullOrEmpty(id));

            mapper.Assign(Sample(), db);

            Assert.Equal(id, db.Id);
            Assert.Equal(id, mapper.CreateModel(db).Id);
            Assert.Equal("UserName", mapper.Map("UserName"));
        }

        [Fact]
        public void RoleMapper_RoundTrip()
        {
            var mapper = new XPRoleMapper<string, PlainRole, XpoDxRole>();
            using var uow = _db.NewUnitOfWork();

            var db = mapper.Assign(new PlainRole { Name = "Cook", NormalizedName = "COOK" }, new XpoDxRole(uow));
            var back = mapper.CreateModel(db);

            Assert.Equal("Cook", back.Name);
            Assert.Equal("COOK", back.NormalizedName);
            Assert.Equal("Name", mapper.Map("Name"));
            Assert.Throws<ArgumentNullException>(() => mapper.Assign(null, new XpoDxRole(uow)));
            Assert.Throws<ArgumentNullException>(() => mapper.Assign(new PlainRole(), null));
        }

        [Fact]
        public void RoleMapper_RoundTrip_KeepsTheConcurrencyStamp()
        {
            var mapper = new XPRoleMapper<string, PlainRole, XpoDxRole>();
            using var uow = _db.NewUnitOfWork();

            var db = mapper.Assign(new PlainRole { Name = "Cook", NormalizedName = "COOK", ConcurrencyStamp = "c" }, new XpoDxRole(uow));
            Assert.Equal("c", mapper.CreateModel(db).ConcurrencyStamp);
        }
    }

    public class RegistrationTests
    {
        [Theory]
        [InlineData(MapperKind.Mapster)]
        [InlineData(MapperKind.AutoMapper)]
        public void Stores_ResolveAsTheXpoStores_PerScope(MapperKind kind)
        {
            using var host = new IdentityHost(kind);
            using var a = host.NewScope();
            using var b = host.NewScope();

            var userStore = a.ServiceProvider.GetRequiredService<IUserStore<IdentityUser>>();
            var roleStore = a.ServiceProvider.GetRequiredService<IRoleStore<IdentityRole>>();

            Assert.Equal(typeof(XPUserStore<,,,,,,,,,,,,>), userStore.GetType().GetGenericTypeDefinition());
            Assert.Equal(typeof(XPRoleStore<,,,,,>), roleStore.GetType().GetGenericTypeDefinition());
            Assert.Same(userStore, a.ServiceProvider.GetRequiredService<IUserStore<IdentityUser>>());
            Assert.NotSame(userStore, b.ServiceProvider.GetRequiredService<IUserStore<IdentityUser>>());
            Assert.IsAssignableFrom<IUserAuthenticationTokenStore<IdentityUser>>(userStore);
            Assert.IsAssignableFrom<IUserTwoFactorRecoveryCodeStore<IdentityUser>>(userStore);
            Assert.IsAssignableFrom<IUserLockoutStore<IdentityUser>>(userStore);
        }

        [Theory]
        [InlineData(MapperKind.Mapster)]
        [InlineData(MapperKind.AutoMapper)]
        public void Validators_AreRegistered(MapperKind kind)
        {
            using var host = new IdentityHost(kind);
            using var scope = host.NewScope();

            Assert.IsType<XPIdentityUserValidator<string, XpoDxUser>>(scope.ServiceProvider.GetRequiredService<IValidator<XpoDxUser>>());
            Assert.IsType<XPIdentityRoleValidator<string, XpoDxRole>>(scope.ServiceProvider.GetRequiredService<IValidator<XpoDxRole>>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<IValidator<XpoDxUserClaim>>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<IValidator<XpoDxUserLogin>>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<IValidator<XpoDxUserToken>>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<IValidator<XpoDxRoleClaim>>());
        }

        [Fact]
        public void AddStores_WithAMissingType_Throws()
        {
            var services = new ServiceCollection();
            Assert.Throws<ArgumentNullException>(() => RegisterIdentityServices.AddStores<string>(services, "x",
                null, typeof(IdentityUserClaim<string>), typeof(IdentityUserLogin<string>), typeof(IdentityUserToken<string>),
                typeof(IdentityUserRole<string>), typeof(IdentityRole), typeof(IdentityRoleClaim<string>),
                typeof(XpoDxUser), typeof(XpoDxUserLogin), typeof(XpoDxUserClaim), typeof(XpoDxUserToken), typeof(XpoDxRole), typeof(XpoDxRoleClaim)));
        }

        [Fact]
        public void AddStores_WithANonIdentityUserType_Throws()
        {
            var services = new ServiceCollection();
            Assert.Throws<InvalidOperationException>(() => RegisterIdentityServices.AddStores<string>(services, "x",
                typeof(string), typeof(IdentityUserClaim<string>), typeof(IdentityUserLogin<string>), typeof(IdentityUserToken<string>),
                typeof(IdentityUserRole<string>), typeof(IdentityRole), typeof(IdentityRoleClaim<string>),
                typeof(XpoDxUser), typeof(XpoDxUserLogin), typeof(XpoDxUserClaim), typeof(XpoDxUserToken), typeof(XpoDxRole), typeof(XpoDxRoleClaim)));
        }

        [Fact]
        public void Bits_FullIncludesUsers()
        {
            Assert.Equal(DxIdentityRoleFlags.FLAG_USERS, DxIdentityRoleFlags.FLAG_FULL & DxIdentityRoleFlags.FLAG_USERS);
        }
    }
}
