using Fasterflect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleRpc
{
    public class RpcServer<TService>
        where TService : class
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
     
        public RpcServer(IServiceProvider serviceProvider, ILogger<RpcServer<TService>> logger)
        {
            this._serviceProvider = serviceProvider;
            this._logger = logger;
        }

        public async Task<RpcResponse> Invoke(RpcRequest request)
        {
            object result = null;
            RpcError rpcError = null;

            if (request != null)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        result = await InvokeInternal(scope.ServiceProvider, request);
                    }
                }
                catch (Exception e)
                {
                    rpcError = new RpcError
                    {
                        Code = RpcErrorCode.RemoteMethodInvocation,
                        Exception = e.Message,
                    };

                    _logger.LogError(e, rpcError.Code.ToString(), request);
                }
            }

            return new RpcResponse
            {
                Result = result,
                Error = rpcError
            };
        }

        private static async Task<object> InvokeInternal(IServiceProvider serviceProvider, RpcRequest request)
        {
            MethodModel methodModel = request.Method;

            Type declaringType = Type.GetType(methodModel.DeclaringType);
            var resolvedType = serviceProvider.GetRequiredService(declaringType);

            Type[] genericArgs = methodModel.GenericArguments.Select(p => Type.GetType(p)).ToArray();
            Type[] paramTypes = methodModel.ParameterTypes.Select(p => Type.GetType(p)).ToArray();

            if (paramTypes.Length != request.Parameters.Length)
            {
                throw new InvalidOperationException("ParamTypes.Length != Parameters.Length");
            }

            object[] parameters = new object[request.Parameters.Length];

            for (int i = 0; i < request.Parameters.Length; i++)
            {
                object p = request.Parameters[i];
                Type type = paramTypes[i];
                if (p != null && p is JsonElement element)
                {
                    parameters[i] = element.Deserialize(type);
                }
            }

            try
            {
                var result = resolvedType.CallMethod(
                    genericArgs,
                    methodModel.MethodName,
                    paramTypes,
                    parameters);

                if (result is Task task)
                {
                    await task;

                    if (!string.IsNullOrEmpty(methodModel.ReturnType))
                    {
                        Type returnType = Type.GetType(methodModel.ReturnType);
                        object res = task.GetPropertyValue(nameof(Task<object>.Result));
                        return res;
                    }

                    return null;
                }

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
