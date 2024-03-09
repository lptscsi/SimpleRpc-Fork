using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleRpc.Serialization;
using System;
using System.IO;
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

                if (rpcError == null)
                {
                    try
                    {
                        rpcRequest = await serializer.Deserialize<RpcRequest>(context.Request.Body);
                        RpcServer rpcServer = new RpcServer(_serviceProvider, _logger);
                        RpcResponse rpcResponse = await rpcServer.Invoke(rpcRequest);
                        context.Response.ContentType = serializer.ContentType;
                        MemoryStream memoryStream = new MemoryStream();
                        await serializer.Serialize(rpcResponse, memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        await memoryStream.CopyToAsync(context.Response.Body);
                    }
                    catch (Exception e)
                    {
                        rpcError = new RpcError
                        {
                            Code = RpcErrorCode.IncorrectRequestBodyFormat,
                            Exception = e.Message,
                        };

                        _logger.LogError(e, rpcError.Code.ToString());
                     
                        if (serializer == null)
                        {
                            serializer = SerializationHelper.GetByName(Constants.DefaultSerializers.Json);
                        }

                        RpcResponse rpcResponse = new RpcResponse()
                        {
                            Error = rpcError,
                            Result  = null
                        };
                        await serializer.Serialize(rpcResponse, context.Response.Body);
                    }
                }
            }
        }
    }
}
