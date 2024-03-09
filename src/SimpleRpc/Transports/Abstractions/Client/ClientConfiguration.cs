namespace SimpleRpc.Transports.Abstractions.Client
{
    public class ClientConfiguration
    {
        public string Name { get; set; }
        
        public BaseClientTransport Transport { get; set; }
    }
}
