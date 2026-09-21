using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace RpcFramework;

public sealed class RuntimeDiagnosticsFeature : IRpcFeatureModule, IRpcRequestHandler
{
	private readonly IRuntimeDiagnosticsBackend backend;

	public RuntimeDiagnosticsFeature(IRuntimeDiagnosticsBackend backend)
	{
		this.backend = backend;
	}

	public void Register(RpcCommandRouter router)
	{
		router.Register("runtimeDiagnostics", this);
	}

	public RpcResult Handle(RpcRequest request, RpcExecutionContext context)
	{
		string text = request.Command ?? request.Body.Value<string>("command") ?? request.Action;
		if (string.IsNullOrWhiteSpace(text))
		{
			throw new InvalidOperationException("runtimeDiagnostics request is missing command.");
		}
		return text switch
		{
			"getConsoleLog" => RpcResult.Success(request, "Runtime console log.", GetConsoleLog(request.Body)), 
			"clearConsoleLog" => ClearConsoleLog(request), 
			"captureScreenshot" => RpcResult.Success(request, "Captured runtime screenshot.", CaptureScreenshot(request.Body)), 
			_ => throw new InvalidOperationException("Unsupported runtimeDiagnostics.command: " + text), 
		};
	}

	public static int LogLevelRank(string? type)
	{
		return type switch
		{
			"Assert" => 4, 
			"Exception" => 4, 
			"Error" => 3, 
			"Warning" => 2, 
			"Log" => 1, 
			_ => 0, 
		};
	}

	private JObject GetConsoleLog(JObject body)
	{
		int sinceSequence = Math.Max(0, body.Value<int?>("sinceSequence").GetValueOrDefault());
		int maxEntries = Math.Clamp(body.Value<int?>("maxEntries") ?? 100, 1, 1000);
		string minLevel = body.Value<string>("minLevel");
		string messageContains = body.Value<string>("messageContains");
		string messageExcludeContains = body.Value<string>("messageExcludeContains");
		return new JObject { ["logs"] = new JArray(from entry in backend.GetConsoleLog(sinceSequence, maxEntries, minLevel, messageContains, messageExcludeContains)
			select entry.ToJObject()) };
	}

	private RpcResult ClearConsoleLog(RpcRequest request)
	{
		backend.ClearConsoleLog();
		return RpcResult.Success(request, "Cleared runtime console log.", new JObject { ["cleared"] = true });
	}

	private JObject CaptureScreenshot(JObject body)
	{
		RuntimeScreenshotRequest runtimeScreenshotRequest = new RuntimeScreenshotRequest
		{
			Width = (body.Value<int?>("width") ?? 1280),
			Height = (body.Value<int?>("height") ?? 720),
			Format = (body.Value<string>("format") ?? "png")
		};
		if (runtimeScreenshotRequest.Width < 1 || runtimeScreenshotRequest.Width > 8192 || runtimeScreenshotRequest.Height < 1 || runtimeScreenshotRequest.Height > 8192)
		{
			throw new InvalidOperationException("Screenshot width and height must be between 1 and 8192.");
		}
		if (!string.Equals(runtimeScreenshotRequest.Format, "png", StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidOperationException("Only png screenshots are supported in v1.");
		}
		return backend.CaptureScreenshot(runtimeScreenshotRequest).ToJObject();
	}
}
