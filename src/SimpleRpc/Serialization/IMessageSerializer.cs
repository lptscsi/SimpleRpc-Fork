using System.IO;
using System.Threading.Tasks;

namespace SimpleRpc.Serialization
{
    public interface IMessageSerializer
    {
        string Name { get; }

        string ContentType { get; }

        Task Serialize<T>(T message, Stream stream);

        Task<T> Deserialize<T>(Stream stream);
    }
}
