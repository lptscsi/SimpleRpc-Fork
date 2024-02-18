using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleRpc.Sample.Shared;
using SimpleRpc.Transports;
using SimpleRpc.Transports.Http.Server;
using SimpleRPC.Sample.Server;
using System.Threading;

namespace SimpleRpc.Sample.Server
{
    public class Startup
    {
        private long _counter;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _counter = 0;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSimpleRpcServer(new HttpServerTransportOptions { Path = "/rpc" });
            
            // to make it more interesting - each instance has its number
            services.AddScoped<IFooService>((sp)=> new FooServiceImpl(Interlocked.Increment(ref _counter)));

            services.AddLogging(options =>
            {
                options.ClearProviders();
                options.AddConsole();
                options.AddFilter("Microsoft", LogLevel.Warning);
                options.AddFilter((logLevel) => logLevel != LogLevel.Information);
            });

            // If using Kestrel:
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSimpleRpcServer();
        }
    }
}
