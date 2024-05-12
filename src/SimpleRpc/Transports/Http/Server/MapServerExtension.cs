using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using SimpleRpc.Serialization;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SimpleRpc.Transports.Http.Server
{
    public static class MapServerExtension
    {
        public static RouteHandlerBuilder MapRpcServer<TService>(this IEndpointRouteBuilder app, [StringSyntax("Route")] string pattern)
            where TService : class
        {
            var value = async Task<Results<FileContentHttpResult, FileStreamHttpResult, BadRequest<string>>> (RpcRequest request, RpcServer<TService> rpcServer, ILogger<RpcServer<TService>> logger) =>
                        {
                            var serializer = SerializationHelper.Json;
                            try
                            {
                                object rpcResponse = await rpcServer.Invoke(request);

                                if (rpcResponse is RpcResponse response)
                                {
                                    MemoryStream memoryStream = new MemoryStream();
                                    await serializer.Serialize(response, memoryStream);
                                    return TypedResults.Bytes(memoryStream.ToArray(), serializer.ContentType);
                                }
                                else if (rpcResponse is Stream stream)
                                {
                                    return TypedResults.Stream(stream, System.Net.Mime.MediaTypeNames.Application.Octet);
                                }
                                else
                                {
                                    return TypedResults.BadRequest($"Unexpected response type: {rpcResponse?.GetType().Name ?? "null"}");
                                }
                            }
                            catch (Exception e)
                            {
                                logger.LogError(e, "SimpleRpc server unexpected error");

                                RpcError rpcError = new RpcError
                                {
                                    Code = RpcErrorCode.IncorrectRequestBodyFormat,
                                    Exception = e.Message,
                                };

                                //  _logger.LogError(e, rpcError.Code.ToString());

                                RpcResponse rpcResponse = new RpcResponse()
                                {
                                    Error = rpcError,
                                    Result = null
                                };
                                MemoryStream memoryStream = new MemoryStream();
                                await serializer.Serialize(rpcResponse, memoryStream);
                                return TypedResults.Bytes(memoryStream.ToArray(), serializer.ContentType);
                            }
                        };

            return app.MapPost(pattern, value);
        }
    }
}
