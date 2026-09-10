namespace RpcFramework;

public sealed class RuntimeScreenshotRequest
{
	public int Width { get; set; }

	public int Height { get; set; }

	public string Format { get; set; } = "png";
}
