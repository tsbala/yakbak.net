using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Yakbak.Middleware.Extensions;

namespace Yakbak.Middleware
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
            if (_options.TapesDirectory == null)
            {
                throw new Exception("Value for tapes directory must be supplied");
            }

            var hash = context.Request.Hash();
            var tapename = Path.Combine(_options.TapesDirectory, $"{hash}.json");
            if (File.Exists(tapename))
            {
                await context.Response.RenderFromTape(tapename);
            }
            else
            {
                await ProxyRequestAndRecordResponse(context, tapename);
            }
        }

        private async Task ProxyRequestAndRecordResponse(HttpContext context, string tapename)
        {
            using (var buffer = new MemoryStream())
            {
                var stream = context.Response.Body;
                context.Response.Body = buffer;

                await context.ProxyRequest(_options.Uri);
                context.Response.SaveToTape(tapename);

                buffer.Seek(0, SeekOrigin.Begin);
                await buffer.CopyToAsync(stream);
                context.Response.Body = stream;
            }
        }
    }
}