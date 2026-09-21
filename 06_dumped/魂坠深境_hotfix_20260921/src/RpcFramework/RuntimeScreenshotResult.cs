using Newtonsoft.Json.Linq;

namespace RpcFramework;

public sealed class RuntimeScreenshotResult
{
	public string? Path { get; set; }

	public string? MimeType { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public long Bytes { get; set; }

	public JObject ToJObject()
	{
		return new JObject
		{
			["path"] = Path,
			["mimeType"] = MimeType,
			["width"] = Width,
			["height"] = Height,
			["bytes"] = Bytes
		};
	}
}
