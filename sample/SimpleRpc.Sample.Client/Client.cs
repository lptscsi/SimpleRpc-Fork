using SimpleRpc.Sample.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleRpc.Sample.Client
{
    public class Client: IDisposable
    {
        private readonly IFooService service;

        public Client(IFooService service)
        {
            this.service = service;
        }

        public async Task TestMain()
        {
            service.Plus(1, 5);

            Console.WriteLine("Calling Concat Method: " + service.Concat("Foo", "Bar"));

            await service.WriteFooAsync("TaskFoo", "TaskBar");
        }

        public async Task TestConcatAsync(int iterations = 5000)
        {
            var startTime = DateTime.Now;
            
            Console.WriteLine($"Start ConcatAsync Iterations: {iterations}");

            for (int i = 0; i < iterations; ++i)
            {
                string res1 = await service.ConcatAsync("sadasd", "asdsd");
            }

            var diff = DateTime.Now - startTime;
            Console.WriteLine($"End ConcatAsync: Time {diff}, Performance: {(iterations / diff.TotalMilliseconds) * 1000} msg/sec");
        }

        public async Task TestReturnGenericType(int iterations = 100)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < 10000; ++i)
            {
                list.Add($"Item {i} RRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRR VVVVVVVVVVVVVVVVVVVVVVVV ФФФФФФФФФФФФФФФФФФФФФФФФФФФФФ TTTTTTTTTTTTTTTTTTTTT");
            }

            var res2 = await service.ReturnGenericType<string>(list);

            Console.WriteLine($"ReturnGenericType Count: {res2?.Count}");

            var startTime = DateTime.Now;
            Console.WriteLine($"Start ReturnGenericType Iterations: {iterations}");

            for (int i = 0; i < iterations; ++i)
            {
                await service.ReturnGenericType<string>(list);
            }

            var diff = DateTime.Now - startTime;
            Console.WriteLine($"End ReturnGenericType: Time {diff}, Performance: {(iterations / diff.TotalMilliseconds) * 1000} msg/sec");
        }

        public async Task TestExceptions()
        {
            try
            {
                await service.ThrowException<object>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Dispose()
        {
            Console.WriteLine("Client Dispose");
        }
    }
}
