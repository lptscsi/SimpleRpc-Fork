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

        public string ContentType => "application/json";

        public Task Serialize<T>(T message, Stream stream)
        {
            using var writer = new Utf8JsonWriter(stream);
            JsonSerializer.Serialize(writer, message);
            return Task.CompletedTask;
        }

        public Task<T> Deserialize<T>(Stream stream)
        {
            return Task.FromResult(JsonSerializer.Deserialize<T>(stream));
        }
    }
}
