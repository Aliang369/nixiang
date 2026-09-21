using Newtonsoft.Json.Linq;

namespace RpcFramework;

public sealed class RuntimeConsoleLogEntry
{
	public int Sequence { get; set; }

	public string? Type { get; set; }

	public string? Message { get; set; }

	public string? StackTrace { get; set; }

	public string? Time { get; set; }

	public JObject ToJObject()
	{
		return new JObject
		{
			["sequence"] = Sequence,
			["type"] = Type,
			["message"] = Message,
			["stackTrace"] = StackTrace,
			["time"] = Time
		};
	}
}
