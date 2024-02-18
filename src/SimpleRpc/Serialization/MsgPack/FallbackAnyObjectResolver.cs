using MessagePack;
using MessagePack.Formatters;

namespace SimpleRpc.Serialization.MsgPack
{
    public class FallbackAnyObjectResolver : IFormatterResolver
    {
        public static readonly FallbackAnyObjectResolver Instance = new FallbackAnyObjectResolver();

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return TypelessGenericObjectFormatter<T>.instance;
        }

        public class TypelessGenericObjectFormatter<T> : IMessagePackFormatter<T>
        {
            public static TypelessGenericObjectFormatter<T> instance = new TypelessGenericObjectFormatter<T>();

            public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
            {
                TypelessFormatter.Instance.Serialize(ref writer, value, options);
            }

            T IMessagePackFormatter<T>.Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
            {
                return (T)TypelessFormatter.Instance.Deserialize(ref reader, options);
            }
        }
    }
}
