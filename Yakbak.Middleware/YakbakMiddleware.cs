using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Yakbak.Middleware.Extensions;

namespace Yakbak.Middleware
{
    public class YakbakMiddleware
    {
        private readonly YakbakOptions _options;
        private readonly ILogger<YakbakMiddleware> _logger;
        private readonly RequestDelegate _next;

        public YakbakMiddleware(YakbakOptions options, ILogger<YakbakMiddleware> logger, RequestDelegate next)
        {
            _options = options;
            _logger = logger;
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
            _logger.LogInformation($"tapename is {tapename}");
            if (File.Exists(tapename))
            {
                _logger.LogInformation($"tapename {tapename} exists");
                await context.Response.RenderFromTape(tapename);
            }
            else
            {
                _logger.LogInformation($"tapename {tapename} does not exist, proxying request and saving to tape");
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