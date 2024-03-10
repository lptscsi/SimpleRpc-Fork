using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SimpleRpc.Transports.Abstractions.Server;
using System;

namespace SimpleRpc.Transports.Http.Server
{
    public class HttpServerTransport<TService> : IServerTransport
        where TService : class
    {
        public void ConfigureServices<T>(IServiceCollection services, IServerTransportOptions<T> serverTransportOptions) 
            where T : class, IServerTransport, new()
        {
            if (serverTransportOptions.GetType() != typeof(HttpServerTransportOptions<TService>))
            {
                throw new ArgumentException("Options has not supported type");
            }

            services.AddSingleton(serverTransportOptions.GetType(), sp => serverTransportOptions);
            services.AddSingleton<RpcServer<TService>>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<HttpTransportMidleware<TService>>();
        }
    }
}
