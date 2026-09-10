using Newtonsoft.Json.Linq;

namespace RpcFramework;

public sealed class RpcError
{
	public string? Message { get; set; }

	public string? Path { get; set; }

	public string? StackTrace { get; set; }

	public JObject? Details { get; set; }

	public JObject ToJObject()
	{
		return new JObject
		{
			["message"] = Message,
			["path"] = Path,
			["stackTrace"] = StackTrace,
			["details"] = Details
		};
	}
}
