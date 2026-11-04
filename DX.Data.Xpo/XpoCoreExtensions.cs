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
				var connStr = sp.GetRequiredService<IConfiguration>().GetConnectionString(connectionName)??"";
                return new XpoDatabase(new XpoDatabaseOptions { ConnectionString = connStr, Name = connectionName });
			});
        }
        public static IServiceCollection AddXpoUnitOfWork(this IServiceCollection serviceCollection, string connectionName)
        {
            return serviceCollection.AddScoped<UnitOfWork>((sp) =>
            {
                var db = sp.GetRequiredService<XpoDatabase>();
                // a name that is not one of the registered database's data layers falls back to its default
                // (first) data layer, as before
                if (!string.IsNullOrEmpty(connectionName) && connectionName != db.DataLayerName && db.IsConfiguredDataLayer(connectionName))
                    return new UnitOfWork(db.GetDataLayer(connectionName));
                return db.GetUnitOfWork();
            });
        }
    }
#endif
}

