using System;
using System.Linq;
using DevExpress.Xpo;

#if (NETSTANDARD2_0)
using Microsoft.Extensions.DependencyInjection;
#endif

namespace DX.Data.Xpo
{
#if (NETSTANDARD2_0)
    public static class XpoCoreExtensions
    {
        public static IServiceCollection AddXpoDatabase(this IServiceCollection serviceCollection, string connectionName, string connectionString)
        {
            //serviceCollection.

            return serviceCollection.AddSingleton<XpoDatabase>(new XpoDatabase(connectionString, connectionName));
        }
        public static IServiceCollection AddXpoUnitOfWork(this IServiceCollection serviceCollection, string connectionName)
        {
            return serviceCollection.AddScoped<UnitOfWork>((sp) => sp.GetService<XpoDatabase>().GetUnitOfWork());
        }
    }
#endif
}

