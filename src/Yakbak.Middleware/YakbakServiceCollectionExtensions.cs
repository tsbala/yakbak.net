using System;
using Microsoft.Extensions.DependencyInjection;

namespace Yakbak.Middleware
{
    public static class YakbakServiceCollectionExtensions
    {
        public static IServiceCollection AddYakBak(this IServiceCollection services,
            Action<YakbakOptions> configureOptions)
        {
            services.Configure(configureOptions);
            return services.AddSingleton<YakbakService>();
        }

        public static IServiceCollection AddYakBak(this IServiceCollection services)
        {
            return services.AddSingleton<YakbakService>();
        }
    }
}