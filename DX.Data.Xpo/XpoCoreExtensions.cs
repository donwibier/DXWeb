using System;
using System.Linq;
using DevExpress.Xpo;

#if (NETSTANDARD2_0)
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
#endif

namespace DX.Data.Xpo
{
#if (NETSTANDARD2_0)
    public static class XpoCoreExtensions
    {
        public static IServiceCollection AddXpoDatabase(this IServiceCollection services, string connectionName/*, string connectionString*/)
		{
			return services.AddSingleton<XpoDatabase>((sp) => {
				var connStr = sp.GetService<IConfiguration>().GetConnectionString(connectionName);
				return new XpoDatabase(connStr, connectionName);
			});
        }
        public static IServiceCollection AddXpoUnitOfWork(this IServiceCollection serviceCollection, string connectionName)
        {
            return serviceCollection.AddScoped<UnitOfWork>((sp) => sp.GetService<XpoDatabase>().GetUnitOfWork());
        }
    }
#endif
}

