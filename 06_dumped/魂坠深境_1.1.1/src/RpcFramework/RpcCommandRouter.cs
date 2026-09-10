using System;
using System.Collections.Generic;

namespace RpcFramework;

public sealed class RpcCommandRouter
{
	private readonly Dictionary<string, IRpcRequestHandler> handlers = new Dictionary<string, IRpcRequestHandler>(StringComparer.Ordinal);

	public void Register(string tool, IRpcRequestHandler handler)
	{
		handlers[tool] = handler;
	}

	public RpcResult Dispatch(RpcRequest request, RpcExecutionContext context)
	{
		if (!handlers.TryGetValue(request.Tool, out IRpcRequestHandler value))
		{
			throw new InvalidOperationException("Unsupported tool: " + request.Tool);
		}
		return value.Handle(request, context);
	}
}
