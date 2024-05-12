using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleRpc.Serialization.Json
{
    public class JsonMessageSerializer : IMessageSerializer
    {
        
        static JsonMessageSerializer()
        {
            
        }

        public string Name => Constants.DefaultSerializers.Json;

        public string ContentType => System.Net.Mime.MediaTypeNames.Application.Json;

        public async Task Serialize<T>(T message, Stream stream)
        {
            await JsonSerializer.SerializeAsync(stream, message);
        }

        public async Task<T> Deserialize<T>(Stream stream)
        {
            return await JsonSerializer.DeserializeAsync<T>(stream);
        }

        public T UnpackResult<T>(RpcRequest rpcRequest, RpcResponse rpcResponse)
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

        public object[] UnpackParameters(object[] parameters, Type[] paramTypes)
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
