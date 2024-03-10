using SimpleRpc.Serialization.Json;

namespace SimpleRpc.Serialization
{
    public static class SerializationHelper
    {
        public static readonly IMessageSerializer Json = new JsonMessageSerializer();
    }
}
