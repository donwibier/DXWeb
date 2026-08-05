#if !NET462
using Microsoft.Extensions.DependencyInjection;

namespace DX.Utils.CronJobs
{
    /// <summary>   <c>IServiceCollection</c> registration for <see cref="CronJobService"/>. Not
    ///             available on .NET Framework builds - use <see cref="CronJobService.Default"/>
    ///             there instead. </summary>
    public static class CronJobServiceExtensions
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Registers <see cref="CronJobService"/> as a singleton so it can be injected
        ///             wherever cron jobs need to be added, removed, or queried at runtime. </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static IServiceCollection AddCronJobService(this IServiceCollection services)
        {
            return services.AddSingleton<CronJobService>();
        }
    }
}
#endif
