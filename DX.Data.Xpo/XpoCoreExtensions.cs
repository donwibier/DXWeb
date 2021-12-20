using System;
using System.Linq;
using DevExpress.Xpo;

#if (NETSTANDARD2_1 || NETCOREAPP)
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
#endif

namespace DX.Data.Xpo
{
#if (NETSTANDARD2_1 || NETCOREAPP)
    public static class XpoCoreExtensions
    {
        public static IServiceCollection AddXpoDatabase(this IServiceCollection services, Action<XpoDatabaseOptions> setupAction)
        {
            return services.AddSingleton<XpoDatabase>((sp => {
                return new XpoDatabase(setupAction);
            }));
        }
        public static IServiceCollection AddXpoDatabases(this IServiceCollection services, Action<XpoDatabaseOptions>[] setupActions)
        {
            return services.AddSingleton<XpoDatabase>((sp => {
                return new XpoDatabase(setupActions);
            }));
        }
        [Obsolete("Please use the AddXpoDatabase(o=>new XpoDatabaseOptions{...}")]
        public static IServiceCollection AddXpoDatabase(this IServiceCollection services, string connectionName/*, string connectionString*/)
		{
			return services.AddSingleton<XpoDatabase>((sp) => {
				var connStr = sp.GetService<IConfiguration>().GetConnectionString(connectionName);
                return new XpoDatabase(new XpoDatabaseOptions { ConnectionString = connStr, Name = connectionName });
			});
        }
        public static IServiceCollection AddXpoUnitOfWork(this IServiceCollection serviceCollection, string connectionName)
        {
            return serviceCollection.AddScoped<UnitOfWork>((sp) => sp.GetService<XpoDatabase>().GetUnitOfWork());
        }
    }
#endif
}

