using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stockpile.Api.App;

namespace Stockpile.Api
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables();
            
            Configuration = builder.Build();
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddTransient<HttpContextService>();
            services.Configure<StockpileOptions>(Configuration);
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(LastChanceExceptionHandler));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IOptions<StockpileOptions> options)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            // Add MVC to the request pipeline.
            app.UseMvc();
        }
    }
}
