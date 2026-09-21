using System;

namespace RpcFramework;

public sealed class DelegateRpcHandler : IRpcRequestHandler
{
	private readonly Func<RpcRequest, RpcExecutionContext, RpcResult> handle;

	public DelegateRpcHandler(Func<RpcRequest, RpcExecutionContext, RpcResult> handle)
	{
		this.handle = handle;
	}

	public RpcResult Handle(RpcRequest request, RpcExecutionContext context)
	{
		return handle(request, context);
	}
}
