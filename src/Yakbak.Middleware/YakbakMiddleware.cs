using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Yakbak.Middleware.Extensions;

namespace Yakbak.Middleware
{
    public class YakbakMiddleware
    {
        private readonly ILogger<YakbakMiddleware> _logger;
        private readonly RequestDelegate _next;

        public YakbakMiddleware(ILogger<YakbakMiddleware> logger, RequestDelegate next)
        {
            _logger = logger;
            _next = next;
        }

        public async Task Invoke(HttpContext context, IOptions<YakbakOptions> options)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (options.Value.TapesDirectory == null)
            {
                throw new ArgumentNullException("TapesDirectory", "Value for tapes directory must be supplied");
            }

            if (options.Value.Uri == null)
            {
                throw new ArgumentNullException("Uri", "Value for Uri must be supplied");
            }

            var proxyRequest = context.CreateProxyHttpRequest(options.Value.Uri);
            var hash = await proxyRequest.Hash(_logger);

            var tapename = Path.Combine(options.Value.TapesDirectory, $"{hash}.json");
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

        private static async Task ProxyRequestAndRecordResponse(HttpContext context, HttpRequestMessage request, string tapename)
        {
            using (var buffer = new MemoryStream())
            {
                // replace the Response.Body with the buffer.
                var stream = context.Response.Body;
                context.Response.Body = buffer;
                
                // Proxy request
                using (var responseMessage = await context.SendProxyHttpRequest(request))
                {
                    // Copy proxy response to the response
                    await context.CopyProxyHttpResponse(responseMessage);
                    // Save response to tape
                    await responseMessage.SaveToTape(tapename);
                }
                
                // reset to original Response body after copying 
                // from buffer
                buffer.Seek(0, SeekOrigin.Begin);
                await buffer.CopyToAsync(stream);
                context.Response.Body = stream;
            }
        }
    }
}