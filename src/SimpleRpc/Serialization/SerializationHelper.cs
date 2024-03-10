using SimpleRpc.Serialization.Json;
using System;
using System.Text.Json;

namespace SimpleRpc.Serialization
{
    public static class SerializationHelper
    {
        public static readonly IMessageSerializer Json = new JsonMessageSerializer();

        public static T UnpackResult<T>(RpcRequest rpcRequest, RpcResponse rpcResponse)
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

        public static object[] UnpackParameters(object[] parameters, Type[] paramTypes)
        {
            // Just as a Guard
            if (parameters.Length != paramTypes.Length)
            {
                throw new ArgumentException("parameters.Length != paramTypes.Length");
            }

            object[] result = new object[parameters.Length];

            if (result.Length == 0)
            {
                return result;
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                object p = parameters[i];
                Type ptype = paramTypes[i];

                if (ptype == null)
                {
                    throw new InvalidOperationException($"Parameter type No:{i} is null");
                }

                if (p != null && p is JsonElement element)
                {
                    result[i] = element.Deserialize(ptype);
                }
                else
                {
                    result[i] = p;
                }
            }

            return result;
        }
    }
}
