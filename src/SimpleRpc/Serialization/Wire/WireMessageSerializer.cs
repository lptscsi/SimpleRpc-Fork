using SimpleRpc.Serialization.Wire.Library;
using System.IO;
using System.Threading.Tasks;

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

        public Task Serialize<T>(T message, Stream stream)
        {
            _serializer.Serialize(message, stream);
            return Task.CompletedTask;
        }

        public Task<T> Deserialize<T>(Stream stream)
        {
            return Task.FromResult(_serializer.Deserialize<T>(stream));
        }
    }
}
