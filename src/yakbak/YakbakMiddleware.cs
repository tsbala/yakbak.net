using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure;

namespace Yakbak
{
    public class YakbakMiddleware
    {
        private readonly YakbakOptions _options;
        private readonly RequestDelegate _next;

        public YakbakMiddleware(YakbakOptions options, RequestDelegate next)
        {
            _options = options;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            HydrateFromRequest(request);
            var host = new HostString(_options.HostName);
            var scheme = _options.Scheme;
            var uri = new Uri(UriHelper.BuildAbsolute(scheme, host, request.PathBase, request.Path, request.QueryString));
            await context.ProxyRequest(uri);
        }

        private void HydrateFromRequest(HttpRequest request)
        {
            if (!string.IsNullOrEmpty(request.Headers["Host"]))
            {
                _options.HostName = request.Headers["Host"];
            }

            if (!string.IsNullOrEmpty(request.Headers["scheme"]))
            {
                _options.Scheme = request.Headers["scheme"];
            }
            else
            {
                _options.Scheme = "http";
            }
        }
    }

    public class YakbakOptions
    {
        public string HostName { get; set; }
        public string Scheme { get; set; } = "http";
    }
}