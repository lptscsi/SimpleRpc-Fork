namespace SimpleRpc
{
    public record RpcRequest
    {
        public MethodModel Method { get; init; }

        public object[] Parameters { get; init; }
    }
}
