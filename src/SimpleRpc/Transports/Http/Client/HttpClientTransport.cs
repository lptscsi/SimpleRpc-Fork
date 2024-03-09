using SimpleRpc.Serialization;
using SimpleRpc.Transports.Abstractions.Client;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
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

                    var result = await resultSerializer.Deserialize<RpcResponse>(stream);

                    if (result.Error != null)
                    {
                        throw new RpcException(result.Error);
                    }

                    try
                    {
                        if (result.Result is JsonElement element)
                        {
                            object val = null;
                            if (!string.IsNullOrEmpty(rpcRequest.Method.ReturnType))
                            {
                                Type type = Type.GetType(rpcRequest.Method.ReturnType);
                                val = element.Deserialize(type);
                            }
                  
                            if (val == null)
                            {
                                return default(T);
                            }

                            return (T)val;
                        }
                        else
                        {
                            return default(T);
                        }
                    }
                    catch(Exception ex)
                    {
                        throw;
                    }
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
            MemoryStream memoryStream = new MemoryStream();
            _serializer.Serialize(_request, memoryStream);
            memoryStream.Position = 0;
            //string data = UTF8Encoding.UTF8.GetString( memoryStream.ToArray());
            await memoryStream.CopyToAsync(stream);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }

}
