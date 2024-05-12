using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleRpc.Sample.Shared;
using SimpleRpc.Serialization;
using SimpleRpc.Transports;
using SimpleRpc.Transports.Http.Server;
using SimpleRPC.Sample.Server;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleRpc.Sample.Server
{
    public class Program
    {
        private static long _counter;

        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Logging
                .ClearProviders()
                .AddConsole()
                .AddFilter("Microsoft", LogLevel.Warning)
            .AddFilter((logLevel) => logLevel != LogLevel.Information);

            builder.Services.AddScoped<IFooService>((sp) => new FooService(Interlocked.Increment(ref _counter)));
            builder.Services.AddSimpleRpcServer<IFooService>();

            using var app = builder.Build();

            app.MapRpcServer<IFooService>("/rpc");
            app.Run();
        }
    }
}
