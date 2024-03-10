using Fasterflect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleRpc
{
    public class RpcServer<TService>
        where TService : class
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        private static ConcurrentDictionary<string, MethodModelCache> _metadata = new ConcurrentDictionary<string, MethodModelCache>();

        static RpcServer()
        {
            MethodInfo[] methods = typeof(TService).GetMethods();
            foreach(MethodInfo method in methods)
            {
                _metadata.TryAdd(method.Name, new MethodModelCache(method));
            }
        }

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
            // We check that the method exists
            if  (!_metadata.TryGetValue(request.Method.MethodName, out MethodModelCache methodModel))
            {
                throw new InvalidOperationException($"Service does not have a method {request.Method.MethodName}");
            }
            // we check that the declaring type is te same what the client has sent
            if (request.Method.DeclaringType != methodModel.Model.DeclaringType)
            {
                throw new InvalidOperationException($"Invalid method parameters for method {methodModel.MethodName}");
            }
            // we check that the number of parameters equals what the client has sent
            if (request.Method.ParameterTypes.Length != methodModel.ParameterTypes.Length)
            {
                throw new InvalidOperationException($"Invalid method parameters for method {methodModel.MethodName}");
            }

            Type declaringType = methodModel.DeclaringType;
            var resolvedType = serviceProvider.GetRequiredService(declaringType);

            // we need this because we need generic parameters which are set on the client side
            MethodModel clientSideMethodModel = request.Method;
            Type[] genericArgs = clientSideMethodModel.GenericArguments.Select(p => Type.GetType(p)).ToArray();
            Type[] paramTypes = clientSideMethodModel.ParameterTypes.Select(p => Type.GetType(p)).ToArray();

            try
            {
                if (paramTypes.Length != request.Parameters.Length)
                {
                    throw new InvalidOperationException("ParamTypes.Length != Parameters.Length");
                }

                object[] parameters = new object[request.Parameters.Length];

                for (int i = 0; i < request.Parameters.Length; i++)
                {
                    object p = request.Parameters[i];
                    Type type = paramTypes[i];
                    if (type == null)
                    {
                        throw new InvalidOperationException($"Parameter type No:{i} for Method {methodModel.MethodName} is null");
                    }
                    if (p != null && p is JsonElement element)
                    {
                        parameters[i] = element.Deserialize(type);
                    }
                }

                var result = resolvedType.CallMethod(
                    genericArgs,
                    methodModel.MethodName,
                    paramTypes,
                    parameters);

                if (result is Task task)
                {
                    await task;

                    if (!string.IsNullOrEmpty(clientSideMethodModel.ReturnType))
                    {
                        Type returnType = Type.GetType(clientSideMethodModel.ReturnType);
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
