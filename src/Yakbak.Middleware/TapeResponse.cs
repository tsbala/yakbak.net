using System.Collections.Generic;

namespace Yakbak.Middleware
{
    public class TapeResponse
    {
        public int StatusCode { get; set; }

        public KeyValuePair<string, string>[] Headers { get; set; }

        public string Base64Body { get; set; }
    }
}