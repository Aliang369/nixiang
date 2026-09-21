namespace RpcFramework;

public sealed class RuntimeScriptExecuteOptions
{
	public bool Enabled { get; set; }

	public int MaxBodyLength { get; set; } = 8192;

	public int DefaultTimeoutMs { get; set; } = 1000;

	public int MaxTimeoutMs { get; set; } = 5000;
}
