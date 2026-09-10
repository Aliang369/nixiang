using System;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace RpcFramework;

public sealed class RuntimeCommandFeature : IRpcFeatureModule, IRpcRequestHandler
{
	private readonly IRuntimeCommandBackend backend;

	public RuntimeCommandFeature(IRuntimeCommandBackend backend)
	{
		this.backend = backend;
	}

	public void Register(RpcCommandRouter router)
	{
		router.Register("runtimeCommand", this);
	}

	public RpcResult Handle(RpcRequest request, RpcExecutionContext context)
	{
		string text = request.Command ?? request.Body.Value<string>("command") ?? request.Action;
		if (string.IsNullOrWhiteSpace(text))
		{
			throw new InvalidOperationException("runtimeCommand request is missing command.");
		}
		return text switch
		{
			"getStatus" => RpcResult.Success(request, "Runtime status.", backend.GetStatus()), 
			"getSceneStatus" => RpcResult.Success(request, "Runtime scene status.", backend.GetSceneStatus()), 
			"findObjects" => RpcResult.Success(request, "Runtime objects.", new JObject { ["objects"] = backend.FindObjects(request.Body) }), 
			"findUiElements" => RpcResult.Success(request, "Runtime UI elements.", new JObject { ["elements"] = new JArray(from element in backend.FindUiElements(request.Body)
				select element.ToJObject()) }), 
			"clickUi" => RpcResult.Success(request, "Clicked runtime UI element.", backend.ClickUi(request.Body)), 
			"invokeRegistered" => RpcResult.Success(request, "Invoked registered runtime command.", backend.InvokeRegistered(request.Body)), 
			"waitForCondition" => RpcResult.Success(request, "Runtime condition satisfied.", WaitForCondition(request.Body, context)), 
			_ => throw new InvalidOperationException("Unsupported runtimeCommand.command: " + text), 
		};
	}

	private JObject WaitForCondition(JObject request, RpcExecutionContext context)
	{
		int num = Math.Max(0, request.Value<int?>("timeoutMs") ?? 1000);
		int millisecondsTimeout = Math.Clamp(request.Value<int?>("pollIntervalMs") ?? 10, 1, 100);
		DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow.AddMilliseconds(num);
		context.ReportPhase("WaitForCondition", "running", request.Value<string>("name"));
		do
		{
			if (backend.IsConditionSatisfied(request))
			{
				return new JObject
				{
					["satisfied"] = true,
					["name"] = request.Value<string>("name")
				};
			}
			Thread.Sleep(millisecondsTimeout);
		}
		while (DateTimeOffset.UtcNow <= dateTimeOffset);
		throw new TimeoutException("Timed out waiting for runtime condition: " + request.Value<string>("name"));
	}
}
