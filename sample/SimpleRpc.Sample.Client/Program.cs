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
        static async Task Main(string[] args)
        {
            Console.WriteLine("Wait a little ...");
            await Task.Delay(2000);
            
            var sc = new ServiceCollection();

            sc.AddSimpleRpcClient("sample", new HttpClientTransportOptions
            {
               Url = "http://127.0.0.1:5000/rpc"
            });

            sc.AddSimpleRpcProxy<IFooService>("sample");
            sc.AddTransient<Client>();

            using (var pr = sc.BuildServiceProvider())
            {
                Func<Task> action = async () =>
                {
                    using (var scope = pr.CreateScope())
                    {
                        var provider = scope.ServiceProvider;
                        var client = provider.GetService<Client>();
                        await client.TestMain();
                        await client.TestConcatAsync();
                        await client.TestReturnGenericType();
                        await client.TestExceptions();
                    }
                };

                List<Task> tasks = new List<Task>();
                for (int i = 0; i < 6; ++i)
                {
                    tasks.Add(action());
                }

                await Task.WhenAll(tasks);
            }

            Console.ReadLine();
        }
    }
}
