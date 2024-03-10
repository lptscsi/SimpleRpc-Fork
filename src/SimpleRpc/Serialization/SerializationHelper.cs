using SimpleRpc.Serialization.Json;
using System.Text.Json;
using System;

namespace SimpleRpc.Serialization
{
    public static class SerializationHelper
    {
        public static readonly IMessageSerializer Json = new JsonMessageSerializer();

        public static T GetResult<T>(RpcRequest rpcRequest, RpcResponse rpcResponse)
        {
            if (rpcResponse.Result is JsonElement element)
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
    }
}
