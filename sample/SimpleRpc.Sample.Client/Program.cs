using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleRpc.Sample.Shared;
using SimpleRpc.Transports;
using SimpleRpc.Transports.Http.Client;

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
               Url = "http://127.0.0.1:5000/rpc",
              // Serializer = "wire"
            });

            sc.AddSimpleRpcProxy<IFooService>("sample");
            // or
            //sc.AddSimpleRpcProxy(typeof(IFooService), "sample");

            var pr = sc.BuildServiceProvider();

            var service = pr.GetService<IFooService>();


            service.Plus(1, 5);

            Console.WriteLine("Calling Concat Method: " + service.Concat("Foo", "Bar"));

            await service.WriteFooAsync("TaskFoo", "TaskBar");

            #region Test ConcatAsync
            var startTime = DateTime.Now;
            int iterations = 5000;
            Console.WriteLine($"Start ConcatAsync Iterations: {iterations}");

            for (int i = 0; i < iterations; ++i)
            {
                string res1 = await service.ConcatAsync("sadasd", "asdsd");
            }

            var diff = DateTime.Now - startTime;
            Console.WriteLine($"End ConcatAsync: Time {diff}, Performance: {(iterations / diff.TotalMilliseconds) * 1000} msg/sec");
            #endregion

            #region Test ReturnGenericType
            List<string> list = new List<string>();
            for(int i = 0; i < 10000; ++i)
            {
                list.Add($"Item {i} RRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRR VVVVVVVVVVVVVVVVVVVVVVVV ФФФФФФФФФФФФФФФФФФФФФФФФФФФФФ TTTTTTTTTTTTTTTTTTTTT");
            }

            iterations = 100;
            var res2 = await service.ReturnGenericType<string>(list);

            Console.WriteLine($"ReturnGenericType Count: {res2?.Count}");

            startTime = DateTime.Now;
            Console.WriteLine($"Start ReturnGenericType Iterations: {iterations}");

            for (int i = 0; i < iterations; ++i)
            {
                await service.ReturnGenericType<string>(list);
            }

            diff = DateTime.Now - startTime;
            Console.WriteLine($"End ReturnGenericType: Time {diff}, Performance: {(iterations / diff.TotalMilliseconds) * 1000} msg/sec");
            #endregion

            try
            {
                await service.ThrowException<object>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.ReadLine();
        }
    }
}
