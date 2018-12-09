using SimpleRpc.Serialization;
using SimpleRpc.Transports.Abstractions.Client;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleRpc.Transports.Http.Client
{
    public class HttpClientTransport : BaseClientTransport
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMessageSerializer _serializer;
        private  readonly string _clientName;

        public HttpClientTransport(string clientName, IMessageSerializer serializer, IHttpClientFactory httpClientFactory)
        {
            _clientName = clientName;
            _httpClientFactory = httpClientFactory;
            _serializer = serializer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object HandleSync(RpcRequest rpcRequest) => SendRequest<object>(rpcRequest).ConfigureAwait(false).GetAwaiter().GetResult();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Task HandleAsync(RpcRequest rpcRequest) => SendRequest<object>(rpcRequest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Task<T> HandleAsyncWithResult<T>(RpcRequest rpcRequest) => SendRequest<T>(rpcRequest);

        private async Task<T> SendRequest<T>(RpcRequest rpcRequest)
        {
            using (var httpClient = _httpClientFactory.CreateClient(_clientName))
            {
                var streamContent = new SerializableContent(_serializer, rpcRequest);
                using (var httpResponseMessage = await httpClient.PostAsync(string.Empty, streamContent, CancellationToken.None).ConfigureAwait(false))
                {
                    httpResponseMessage.EnsureSuccessStatusCode();

                    var resultSerializer = SerializationHelper.GetByContentType(httpResponseMessage.Content.Headers.ContentType.MediaType);
                    var stream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);

                    var result = (RpcResponse)resultSerializer.Deserialize(stream, typeof(RpcResponse));

                    if (result.Error != null)
                    {
                        throw new RpcException(result.Error);
                    }

                    return (T)result.Result;
                }
            }
        }
    }

    internal class SerializableContent : HttpContent
    {
        private readonly IMessageSerializer _serializer;
        private readonly RpcRequest _request;

        public SerializableContent(IMessageSerializer serializer, RpcRequest request)
        {
            _serializer = serializer;
            _request = request;
            Headers.ContentType = new MediaTypeHeaderValue(_serializer.ContentType);
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            var bufStream = new BufferedStream(stream, 1024);
            _serializer.Serialize(_request, bufStream, typeof(RpcRequest));
            await bufStream.FlushAsync();
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }

}
