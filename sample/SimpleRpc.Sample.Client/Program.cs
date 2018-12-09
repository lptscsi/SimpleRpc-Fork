using Microsoft.Extensions.DependencyInjection;
using SimpleRpc.Sample.Shared;
using SimpleRpc.Transports;
using SimpleRpc.Transports.Http.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleRpc.Sample.Client
{
    class Program
    {
        static ServiceProvider AddServices()
        {
            var sc = new ServiceCollection();
            sc.AddSimpleRpcClient("sample", new HttpClientTransportOptions
            {
                Url = "http://127.0.0.1:5000/rpc"
            });

            sc.AddSimpleRpcProxy<IFooService>("sample");
            sc.AddTransient<Client>();
            return sc.BuildServiceProvider();
        }

        static async Task Main(string[] args)
        {
            using (var rootProvider = AddServices())
            {
                Func<Task> action = async () =>
                {
                    using (var scope = rootProvider.CreateScope())
                    {
                        var provider = scope.ServiceProvider;
                        var client = provider.GetService<Client>();
                        await client.TestMain();
                        await client.TestConcatAsync();
                        await client.TestReturnGenericType();
                        await client.TestExceptions();
                    }
                };

                Console.WriteLine("Wait a little ... for the server be started");
                await Task.Delay(2000);

                List<Task> tasks = new List<Task>();
                for (int i = 0; i < 5; ++i)
                {
                    tasks.Add(action());
                }

                await Task.WhenAll(tasks);
            }

            Console.ReadLine();
        }
    }
}
