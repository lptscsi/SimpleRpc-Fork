using SimpleRpc.Transports.Abstractions.Client;
using System.Collections.Generic;

namespace SimpleRpc.Transports.Http.Client
{
    public class HttpClientTransportOptions<TService> : IClientTransportOptions<HttpClientTransport<TService>>
        where TService : class
    {
        public string Url { get; init; }

        public string ApplicationName { get; init; }

        public IDictionary<string, string> DefaultRequestHeaders { get; init; }

        public string Serializer { get; init; } = Constants.DefaultSerializers.Json;
    }
}
