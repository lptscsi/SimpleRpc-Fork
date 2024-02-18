using MessagePack;
using MessagePack.Formatters;
using SimpleRpc.Serialization.Wire;
using System.Buffers;
using System.IO;

namespace SimpleRpc.Serialization.MsgPack
{
    public class WireFallbackAnyObjectResolver : IFormatterResolver
    {
        public static readonly WireFallbackAnyObjectResolver Instance = new WireFallbackAnyObjectResolver();

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return WireAnyObjectFormatter<T>.instance;
        }

        public class WireAnyObjectFormatter<T> : IMessagePackFormatter<T>
        {
            public static WireAnyObjectFormatter<T> instance = new WireAnyObjectFormatter<T>();

            public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
            {
                if (value == null)
                {
                    writer.WriteNil();
                }

                using (var stream = new MemoryStream())
                {
                    WireMessageSerializer._serializer.Serialize(value, stream);

                    writer.Write(stream.ToArray());
                }
            }

            T IMessagePackFormatter<T>.Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
            {
                if (reader.IsNil)
                {
                    return default(T);
                }

                using (var stream = new MemoryStream())
                {
                    stream.Write(reader.ReadBytes().Value.ToArray());
                    stream.Position = 0;

                    return (T)WireMessageSerializer._serializer.Deserialize(stream);
                }
            }
        }
    }
}
