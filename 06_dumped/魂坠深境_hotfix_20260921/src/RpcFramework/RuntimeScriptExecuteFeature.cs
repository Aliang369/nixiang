using System;

namespace RpcFramework;

public sealed class RuntimeScriptExecuteFeature : IRpcFeatureModule, IRpcRequestHandler
{
	private readonly IRuntimeScriptExecutor executor;

	private readonly RuntimeScriptExecuteOptions options;

	public RuntimeScriptExecuteFeature(IRuntimeScriptExecutor executor, RuntimeScriptExecuteOptions? options = null)
	{
		this.executor = executor;
		this.options = options ?? new RuntimeScriptExecuteOptions();
	}

	public void Register(RpcCommandRouter router)
	{
		router.Register("runtimeScriptExecute", this);
	}

	public RpcResult Handle(RpcRequest request, RpcExecutionContext context)
	{
		string text = request.Command ?? request.Body.Value<string>("command") ?? request.Action;
		if (text != "execute")
		{
			throw new InvalidOperationException("Unsupported runtimeScriptExecute.command: " + text);
		}
		if (!options.Enabled)
		{
			throw new InvalidOperationException("Runtime script execution is disabled.");
		}
		string text2 = request.Body.Value<string>("body") ?? string.Empty;
		if (string.IsNullOrWhiteSpace(text2))
		{
			throw new InvalidOperationException("runtimeScriptExecute body is required.");
		}
		if (text2.Length > options.MaxBodyLength)
		{
			throw new InvalidOperationException("runtimeScriptExecute script body is too long.");
		}
		int value = request.Body.Value<int?>("timeoutMs") ?? options.DefaultTimeoutMs;
		value = Math.Clamp(value, 1, options.MaxTimeoutMs);
		context.ReportPhase("ExecuteRuntimeScript", "running", "body");
		return RpcResult.Success(request, "Executed runtime script.", executor.ExecuteBody(new RuntimeScriptExecuteRequest
		{
			Body = text2,
			TimeoutMs = value
		}));
	}
}
