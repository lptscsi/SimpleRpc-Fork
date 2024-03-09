using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleRpc.Serialization;
using System;
using System.Threading.Tasks;

namespace SimpleRpc.Transports.Http.Server
{
    internal class HttpTransportMidleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HttpTransportMidleware> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpServerTransportOptions _httpServerTransportOptions;

        public HttpTransportMidleware(
            RequestDelegate next, 
            ILogger<HttpTransportMidleware> logger, 
            IServiceProvider serviceProvider, 
            HttpServerTransportOptions httpServerTransportOptions)
        {
            _next = next;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _httpServerTransportOptions = httpServerTransportOptions;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path != _httpServerTransportOptions.Path)
            {
                await _next(context);
            }
            else
            {
                var rpcRequest = (RpcRequest)null;
                var rpcError = (RpcError)null;
                var result = (object)null;
                var serializer = (IMessageSerializer)null;
                try
                {
                    serializer = SerializationHelper.GetByContentType(context.Request.ContentType);
                }
                catch(Exception ex)
                {
                    rpcError = new RpcError { Code = RpcErrorCode.NotSupportedContentType };
                    _logger.LogError(ex, context.Request.ContentType);
                }

                if (serializer != null)
                {
                    try
                    {
                        rpcRequest = await serializer.Deserialize<RpcRequest>(context.Request.Body);
                    }
                    catch (Exception e)
                    {
                        rpcError = new RpcError
                        {
                            Code = RpcErrorCode.IncorrectRequestBodyFormat,
                            Exception = e.Message,
                        };

                        _logger.LogError(e, rpcError.Code.ToString());
                    }
                }

                if (rpcRequest != null)
                {
                    try
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            result = await rpcRequest.Invoke(scope.ServiceProvider);
                        }
                    }
                    catch (Exception e)
                    {
                        rpcError = new RpcError
                        {
                            Code = RpcErrorCode.RemoteMethodInvocation,
                            Exception = e.Message,
                        };

                        _logger.LogError(e, rpcError.Code.ToString(), rpcRequest);
                    }
                }

                if (serializer == null)
                {
                    serializer = SerializationHelper.GetByName(Constants.DefaultSerializers.Json);
                }

                context.Response.ContentType = serializer.ContentType;
                await serializer.Serialize(
                    new RpcResponse
                    {
                        Result = result,
                        Error = rpcError
                    },
                    context.Response.Body);
            }
        }
    }
}
