using DX.Data.Xpo.Identity.Persistent;
using Xunit;

namespace DX.Data.Xpo.Identity.Tests
{
    public class XpoDxUserTests
    {
        private readonly InMemoryDb _db = new InMemoryDb();

        private string AddUser(string name = "user@example.com")
            => _db.Commit(uow => { var u = InMemoryDb.NewUser(uow, name); uow.CommitChanges(); return u.Id; });

        private void DeleteUser(string id)
            => _db.Commit(uow => { uow.GetObjectByKey<XpoDxUser>(id).Delete(); return 0; });

        [Fact]
        public void Save_GivesTheUserAnIdAndStamps()
        {
            var id = AddUser();

            using var uow = _db.NewUnitOfWork();
            var user = uow.GetObjectByKey<XpoDxUser>(id);
            Assert.False(string.IsNullOrEmpty(id));
            Assert.NotEqual(default, user.AddStampUTC);
            Assert.NotEqual(default, user.ModStampUTC);
        }

        [Fact]
        public void ChangingEmailAndUserName_SetsTheirUpperCaseFields()
        {
            using var uow = _db.NewUnitOfWork();
            var user = InMemoryDb.NewUser(uow, "Mixed.Case@Example.com");

            Assert.Equal("MIXED.CASE@EXAMPLE.COM", user.EmailUpper);
            Assert.Equal("MIXED.CASE@EXAMPLE.COM", user.UserNameUpper);
        }

        [Fact]
        public void Delete_WithoutDefaultSession_DeletesTheUser()
        {
            var id = AddUser();

            DeleteUser(id);

            using var uow = _db.NewUnitOfWork();
            Assert.Null(uow.GetObjectByKey<XpoDxUser>(id));
        }

        [Fact]
        public void Delete_RemovesTheUserFromItsRoles_AndKeepsTheRoles()
        {
            var (userId, roleId, otherId) = _db.Commit(uow =>
            {
                var role = InMemoryDb.NewRole(uow, "Admin");
                var user = InMemoryDb.NewUser(uow, "user@example.com");
                var other = InMemoryDb.NewUser(uow, "other@example.com");
                role.Users.Add(user);
                role.Users.Add(other);
                uow.CommitChanges();
                return (user.Id, role.Id, other.Id);
            });

            DeleteUser(userId);

            using var check = _db.NewUnitOfWork();
            var role = check.GetObjectByKey<XpoDxRole>(roleId);
            Assert.NotNull(role);
            Assert.Collection(role.Users, u => Assert.Equal(otherId, u.Id));
        }

        [Fact]
        public void Delete_DeletesTheUsersClaimsLoginsAndTokens()
        {
            var (userId, otherId) = _db.Commit(uow =>
            {
                var user = InMemoryDb.NewUser(uow, "user@example.com");
                var other = InMemoryDb.NewUser(uow, "other@example.com");
                new XpoDxUserClaim(uow) { User = user, ClaimType = "type", ClaimValue = "mine" };
                new XpoDxUserClaim(uow) { User = other, ClaimType = "type", ClaimValue = "other's" };
                new XpoDxUserLogin(uow) { User = user, LoginProvider = "Google", ProviderKey = "key" };
                new XpoDxUserToken(uow) { User = user, LoginProvider = "Google", Name = "token", Value = "value" };
                uow.CommitChanges();
                return (user.Id, other.Id);
            });

            DeleteUser(userId);

            Assert.Equal(1, _db.Count<XpoDxUserClaim>());
            Assert.Equal(0, _db.Count<XpoDxUserLogin>());
            Assert.Equal(0, _db.Count<XpoDxUserToken>());
            using var check = _db.NewUnitOfWork();
            Assert.Equal("other's", Assert.Single(check.GetObjectByKey<XpoDxUser>(otherId).Claims).ClaimValue);
        }
    }
}
