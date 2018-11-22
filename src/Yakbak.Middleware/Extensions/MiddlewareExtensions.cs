using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Yakbak.Middleware.Extensions
{
    public static class ServerExtensions
    {
        public static void UseYakbak(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.UseMiddleware<YakbakMiddleware>();
        }
    }
}
