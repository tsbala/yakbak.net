using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Yakbak.Middleware.Extensions
{
    public static class RequestExtensions
    {
        public static string Hash(this HttpRequest request)
        {
            using (var md5 = IncrementalHash.CreateHash(HashAlgorithmName.MD5))
            {
                if (request.Body != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        request.Body.CopyTo(ms);
                        md5.AppendData(ms.ReadAllBytes());
                    }
                }

                md5.AppendData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request.Headers)));
                md5.AppendData(Encoding.UTF8.GetBytes(request.Method));
                md5.AppendData(Encoding.UTF8.GetBytes(request.Scheme));
                md5.AppendData(Encoding.UTF8.GetBytes(request.QueryString.Value));

                var result = md5.GetHashAndReset();
                return BitConverter.ToString(result)
                    .Replace("-", string.Empty) // without dashes
                    .ToLower(); // make lowercase
            }
        }
    }
}