using Newtonsoft.Json.Linq;

namespace RpcFramework;

public sealed class RpcRequest
{
	public const string DefaultTool = "skillAssetDsl";

	public const string DefaultAction = "dryRun";

	public string JobId { get; set; } = string.Empty;

	public string Tool { get; set; } = string.Empty;

	public string Action { get; set; } = string.Empty;

	public string? Command { get; set; }

	public string Source { get; set; } = string.Empty;

	public JObject Body { get; set; } = new JObject();

	public static RpcRequest FromJson(string json, string source, string fallbackJobId, string? defaultTool = null, string? defaultAction = null)
	{
		return FromJObject(JObject.Parse(json), source, fallbackJobId, defaultTool, defaultAction);
	}

	public static RpcRequest FromJObject(JObject body, string source, string fallbackJobId, string? defaultTool = null, string? defaultAction = null)
	{
		string text = (string.IsNullOrWhiteSpace(fallbackJobId) ? body.Value<string>("jobId") : fallbackJobId);
		return new RpcRequest
		{
			JobId = (text ?? string.Empty),
			Tool = (body.Value<string>("tool") ?? defaultTool ?? "skillAssetDsl"),
			Action = (body.Value<string>("action") ?? defaultAction ?? "dryRun"),
			Command = body.Value<string>("command"),
			Source = source,
			Body = body
		};
	}
}
