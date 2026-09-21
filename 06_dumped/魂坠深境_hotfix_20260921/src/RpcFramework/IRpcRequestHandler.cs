namespace RpcFramework;

public interface IRpcRequestHandler
{
	RpcResult Handle(RpcRequest request, RpcExecutionContext context);
}
