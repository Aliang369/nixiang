using Newtonsoft.Json.Linq;

namespace RpcFramework;

public sealed class RpcLogEntry
{
	public string? Type { get; set; }

	public string? Message { get; set; }

	public string? StackTrace { get; set; }

	public string? Time { get; set; }

	public JObject ToJObject()
	{
		return new JObject
		{
			["type"] = Type,
			["message"] = Message,
			["stackTrace"] = StackTrace,
			["time"] = Time
		};
	}
}
