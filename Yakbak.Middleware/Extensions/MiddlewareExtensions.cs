using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Yakbak.Middleware.Extensions
{
    public static class ServerExtensions
    {
        public static void UseYakbak(this IApplicationBuilder app, IOptions<YakbakOptions> options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            app.UseMiddleware<YakbakMiddleware>(options.Value);
        }
    }
}
