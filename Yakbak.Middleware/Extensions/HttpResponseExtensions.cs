using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Yakbak.Middleware.Extensions
{
    public static class HttpResponseExtensions
    {
        public static void SaveToTape(this HttpResponse response, string tapeName)
        {
            var tapedResponse = new TapeResponse
            {
                Headers = response.Headers
                                  .Select(x => new KeyValuePair<string, string>(x.Key, x.Value.ToString()))
                                  .ToArray(),
                StatusCode = response.StatusCode
            };
            var bytes = response.Body.ReadAllBytes();
            tapedResponse.Base64Body = Convert.ToBase64String(bytes);

            var directory = Path.GetDirectoryName(tapeName);
            Directory.CreateDirectory(directory);
            if (File.Exists(tapeName))
            {
                File.Delete(tapeName);
            }

            File.WriteAllText(tapeName, JsonConvert.SerializeObject(tapedResponse, Formatting.Indented));
        }

        public static async Task RenderFromTape(this HttpResponse response, string tapeName)
        {
            var tape = JsonConvert.DeserializeObject<TapeResponse>(File.ReadAllText(tapeName));

            foreach (var header in tape.Headers)
            {
                response.Headers.Append(header.Key, new StringValues(header.Value));
            }

            response.StatusCode = tape.StatusCode;
            var bytes = Convert.FromBase64String(tape.Base64Body);
            await response.Body.WriteAsync(bytes);
        }
    }
}