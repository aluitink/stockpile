using System.IO;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;

namespace Stockpile.Api.App
{
    public class Startup
    {
        protected IConfiguration Configuration { get; private set; }
        
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder();

            var configFile = "config.json";
            if (File.Exists(configFile))
                builder.AddJsonFile(configFile);

            builder.AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        // This method gets called by a runtime.
        // Use this method to add services to the container
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

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IOptions<StockpileOptions> options)
        {
            var webConfigPath = Path.Combine(env.WebRootPath, "web.config");
            var logRootDirPath = options.Value.LoggingDirectory;

            loggerFactory.MinimumLevel = LogLevel.Debug;
            loggerFactory.AddProvider(new Log4NetLoggerProvider(new FileInfo(webConfigPath), new DirectoryInfo(logRootDirPath)));
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();
            
            // Add MVC to the request pipeline.
            app.UseMvc();
        }
    }
}
