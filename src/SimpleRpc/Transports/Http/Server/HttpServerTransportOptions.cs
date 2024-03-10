using SimpleRpc.Transports.Abstractions.Server;

namespace SimpleRpc.Transports.Http.Server
{
    public class HttpServerTransportOptions<TService> : IServerTransportOptions<HttpServerTransport<TService>>
        where TService : class
    {
        public string Path { get; init; }

        public string ServiceName { get; init; }
    }
}
