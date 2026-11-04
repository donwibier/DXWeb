using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DX.Data.Xpo.Identity.Mapster;
using DX.Data.Xpo.Identity.Persistent;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DX.Data.Xpo.Identity.Tests
{
    public class MapsterGlobalConfigTests
    {
        private readonly InMemoryDb _db = new InMemoryDb();

        [Fact]
        public async Task Registering_WhileAnotherContainerMaps_DoesNotThrow()
        {
            new ServiceCollection().AddIdentityCore<IdentityUser>().AddRoles<IdentityRole>().AddXpoMapsterIdentityStores("first");
            using var stop = new CancellationTokenSource();
            // what XPMapsterUserStore.MapTo does on every create/update: IdentityUser -> XpoDxUser through the global config
            var mappers = Enumerable.Range(0, 4).Select(_ => Task.Run(() =>
            {
                var mapper = new MapsterMapper.Mapper();
                using var uow = _db.NewUnitOfWork();
                var target = new XpoDxUser(uow);
                while (!stop.IsCancellationRequested)
                    mapper.Map(new IdentityUser("user@example.com"), target);
            })).ToArray();

            try
            {
                for (var i = 0; i < 5000; i++)
                    new ServiceCollection().AddIdentityCore<IdentityUser>().AddRoles<IdentityRole>().AddXpoMapsterIdentityStores("other" + i);
            }
            finally
            {
                stop.Cancel();
                await Task.WhenAll(mappers);
            }
        }
    }
}
