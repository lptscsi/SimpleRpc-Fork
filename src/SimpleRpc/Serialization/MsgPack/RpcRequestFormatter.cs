using MessagePack;
using MessagePack.Formatters;

namespace SimpleRpc.Serialization.MsgPack
{
    internal class RpcRequestFormatter : IMessagePackFormatter<RpcRequest>
    {
        public void Serialize(ref MessagePackWriter writer, RpcRequest value, MessagePackSerializerOptions options)
        {
            options.Resolver.GetFormatter<MethodModel>().Serialize(ref writer, value.Method,options);
            options.Resolver.GetFormatter<object[]>().Serialize(ref writer, value.Parameters, options);
        }

        public RpcRequest Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var methodModel = options.Resolver.GetFormatter<MethodModel>().Deserialize(ref reader, options);
            var objectArray = options.Resolver.GetFormatter<object[]>().Deserialize(ref reader, options);

            return new RpcRequest
            {
                Method = methodModel,
                Parameters = objectArray
            };
        }
    }
}
