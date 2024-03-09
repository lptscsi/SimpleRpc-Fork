using Fasterflect;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SimpleRpc.Transports.Abstractions.Client
{
    public class RoutableProxy : DispatchProxy
    {
        private BaseClientTransport? _transport;

        public RoutableProxy()
        { }

    
        public static T Create<T>(BaseClientTransport transport)
            where T : class
        {
            if (transport == null)
                throw new ArgumentNullException(nameof(transport));

            object proxy = Create<T, RoutableProxy>();
            var routableProxy = (RoutableProxy)proxy;

            routableProxy._transport = transport;

            return (T)proxy;
        }

        /// <inheritdoc/>
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (_transport == null)
                throw new InvalidOperationException("Proxy transport is NULL");
    
            if (targetMethod == null)
            {
                return null;
            }

            return _transport.Invoke(targetMethod, args);
        }
    }

    public abstract class BaseClientTransport : IClientTransport
    {
        public abstract object HandleSync(RpcRequest rpcRequest);

        public abstract Task HandleAsync(RpcRequest rpcRequest);

        public abstract Task<T> HandleAsyncWithResult<T>(RpcRequest rpcRequest);

        public object Invoke(MethodInfo targetMethod, object?[]? args)
        {
            var rpcRequest = new RpcRequest
            {
                Method = new MethodModel(targetMethod, targetMethod.GetGenericArguments()),
                Parameters = args
            };

            if (typeof(Task).IsAssignableFrom(targetMethod.ReturnType))
            {
                //Task<T>
                if (targetMethod.ReturnType.IsGenericType)
                {
                      return this.CallMethod(
                        targetMethod.ReturnType.GetGenericArguments(), 
                        nameof(HandleAsyncWithResult),
                        rpcRequest);
                }
                else
                {
                    //Task
                    return HandleAsync(rpcRequest);
                }
            }
            else
            {
                return HandleSync(rpcRequest);
            }
        }
    }
}
