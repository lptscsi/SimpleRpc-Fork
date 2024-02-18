using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using SimpleRpc.Serialization.Wire.Library.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SimpleRpc.Serialization.MsgPack
{
    public class MsgPackSerializer : IMessageSerializer
    {
        internal static IFormatterResolver _resolver;

        static MsgPackSerializer()
        {
           IFormatterResolver formatterResolver1 = CompositeResolver.Create(
               TypelessContractlessStandardResolver.Instance,
               FallbackAnyObjectResolver.Instance);

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

            IFormatterResolver formatterResolver2 = CompositeResolver.Create(list.ToArray());

            _resolver = CompositeResolver.Create(formatterResolver1, formatterResolver2);
        }

        public string Name => Constants.DefaultSerializers.MessagePack;

        public string ContentType => "application/x-msgpack";

        public Task Serialize<T>(T message, Stream stream)
        {
            //lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            MessagePackSerializerOptions serializerOptions = new MessagePackSerializerOptions(_resolver);
            MessagePackSerializer.Serialize(stream, message, serializerOptions);
            return Task.CompletedTask;
        }

        public Task<T> Deserialize<T>(Stream stream)
        {
            MessagePackSerializerOptions serializerOptions = new MessagePackSerializerOptions(_resolver);
            return Task.FromResult(MessagePackSerializer.Deserialize<T>(stream, serializerOptions));
        }
    }
}
