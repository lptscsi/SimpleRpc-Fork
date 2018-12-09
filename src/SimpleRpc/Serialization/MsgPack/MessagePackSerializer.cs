using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using SimpleRpc.Serialization.Wire.Library.Extensions;

namespace SimpleRpc.Serialization.MsgPack
{
    public class MsgPackSerializer : IMessageSerializer
    {
        internal static IFormatterResolver _resolver;

        static MsgPackSerializer()
        {
            CompositeResolver.Register(TypelessContractlessStandardResolver.Instance, FallbackAnyObjectResolver.Instance);

            var list = new List<IMessagePackFormatter>
            {
                new TypeFormatter<Type>(),
                new RpcRequestFormatter(),
                WireFallbackAnyObjectResolver.WireAnyObjectFormatter<Exception>.instance
            };

            if (TypeEx.TypeType != TypeEx.RuntimeType)
            {
                list.Add((IMessagePackFormatter)Activator.CreateInstance(typeof(TypeFormatter<>).MakeGenericType(TypeEx.RuntimeType)));
            }

            CompositeResolver.Register(list.ToArray());

            _resolver = CompositeResolver.Instance;
        }

        public string Name => Constants.DefaultSerializers.MessagePack;

        public string ContentType => "application/x-msgpack";

        public Task Serialize<T>(T message, Stream stream)
        {
            LZ4MessagePackSerializer.Serialize(stream, message, _resolver);
            return Task.CompletedTask;
        }

        public Task<T> Deserialize<T>(Stream stream)
        {
            return Task.FromResult(LZ4MessagePackSerializer.Deserialize<T>(stream, _resolver));
        }
    }
}
