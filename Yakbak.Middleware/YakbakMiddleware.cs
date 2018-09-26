using System;
using System.IO;
using System.Net.Http;
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
                throw new ArgumentNullException("TapesDirectory", "Value for tapes directory must be supplied");
            }

            if (_options.Uri == null)
            {
                throw new ArgumentNullException("Uri", "Value for Uri must be supplied");
            }

            var proxyRequest = context.CreateProxyHttpRequest(_options.Uri);
            var hash = await proxyRequest.Hash(_logger);

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
                await ProxyRequestAndRecordResponse(context, proxyRequest, tapename);
            }
        }

        private async Task ProxyRequestAndRecordResponse(HttpContext context, HttpRequestMessage request, string tapename)
        {
            using (var buffer = new MemoryStream())
            {
                var stream = context.Response.Body;
                context.Response.Body = buffer;
                using (var responseMessage = await context.SendProxyHttpRequest(request))
                {
                    await responseMessage.SaveToTape(tapename);
                    await context.CopyProxyHttpResponse(responseMessage);
                }
                buffer.Seek(0, SeekOrigin.Begin);
                await buffer.CopyToAsync(stream);
                context.Response.Body = stream;
            }
        }
    }
}