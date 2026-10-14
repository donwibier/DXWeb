using System;
using DX.Data.Xpo.Identity.Persistent;
using Xunit;

namespace DX.Data.Xpo.Identity.Tests
{
    public class XpoDxRoleTests
    {
        private readonly InMemoryDb _db = new InMemoryDb();

        [Fact]
        public void ChangingName_SetsNameUpper()
        {
            using var uow = _db.NewUnitOfWork();
            var role = InMemoryDb.NewRole(uow, "Administrator");

            Assert.Equal("ADMINISTRATOR", role.NameUpper);
        }

        [Fact]
        public void Save_WithoutName_Throws()
        {
            using var uow = _db.NewUnitOfWork();
            new XpoDxRole(uow);

            Assert.ThrowsAny<Exception>(() => uow.CommitChanges());
        }

        [Fact]
        public void Delete_KeepsItsUsers_AndDeletesItsClaims()
        {
            var (roleId, userId) = _db.Commit(uow =>
            {
                var role = InMemoryDb.NewRole(uow, "Admin");
                var user = InMemoryDb.NewUser(uow, "user@example.com");
                role.Users.Add(user);
                new XpoDxRoleClaim(uow) { Role = role, ClaimType = "type", ClaimValue = "value" };
                uow.CommitChanges();
                return (role.Id, user.Id);
            });

            _db.Commit(uow => { uow.GetObjectByKey<XpoDxRole>(roleId).Delete(); return 0; });

            using var check = _db.NewUnitOfWork();
            Assert.Null(check.GetObjectByKey<XpoDxRole>(roleId));
            Assert.Empty(check.GetObjectByKey<XpoDxUser>(userId).Roles);
            Assert.Equal(0, _db.Count<XpoDxRoleClaim>());
        }
    }
}
