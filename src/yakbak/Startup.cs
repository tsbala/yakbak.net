using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Yakbak.Middleware;
using Yakbak.Middleware.Extensions;

namespace Yakbak
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<YakbakOptions>(Configuration.GetSection("Yakbak"));
            services.AddYakBak();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseYakbak();
        }
    }
}
