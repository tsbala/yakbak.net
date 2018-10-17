using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Yakbak.Middleware.Extensions
{
    public static class RequestExtensions
    {
        public static async Task<string> Hash(this HttpRequestMessage request, ILogger logger)
        {
            using (var md5 = IncrementalHash.CreateHash(HashAlgorithmName.MD5))
            {
                logger.LogInformation($"Request path: {request.RequestUri.PathAndQuery}");

                if (request.Content != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        await request.Content.CopyToAsync(ms);
                        md5.AppendData(ms.ReadAllBytes());
                    }
                }

                md5.AppendData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request.Headers)));
                md5.AppendData(Encoding.UTF8.GetBytes(request.Method.Method));
                md5.AppendData(Encoding.UTF8.GetBytes(request.RequestUri.Scheme));
                md5.AppendData(Encoding.UTF8.GetBytes(request.RequestUri.PathAndQuery));

                var result = md5.GetHashAndReset();
                return BitConverter.ToString(result)
                    .Replace("-", string.Empty) // without dashes
                    .ToLower(); // make lowercase
            }
        }
    }
}