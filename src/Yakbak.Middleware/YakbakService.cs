using System.Net.Http;
using Microsoft.Extensions.Options;

namespace Yakbak.Middleware
{
    public class YakbakService
    {
        public YakbakService(IOptions<YakbakOptions> options)
        {
            YakbakOptions = options.Value;
            var handler = new HttpClientHandler() { AllowAutoRedirect =  false, UseCookies = false };
            Client = new HttpClient(handler);
        }
        
        public YakbakOptions YakbakOptions { get; private set; }
        internal HttpClient Client { get; private set; }
    }
}