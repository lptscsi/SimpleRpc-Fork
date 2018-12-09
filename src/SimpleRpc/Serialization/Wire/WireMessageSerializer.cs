using SimpleRpc.Serialization.Wire.Library;
using System;
using System.IO;

namespace SimpleRpc.Serialization.Wire
{
    public class WireMessageSerializer : IMessageSerializer
    {
        internal static Serializer _serializer;

        static WireMessageSerializer()
        {
            _serializer = new Serializer();
        }

        public string Name => Constants.DefaultSerializers.Wire;

        public string ContentType => "application/x-wire";

        public void Serialize(object message, Stream stream, Type type)
        {
            _serializer.Serialize(message, stream);
        }

        public object Deserialize(Stream stream, Type type)
        {
            return _serializer.Deserialize(stream);
        }
    }
}
