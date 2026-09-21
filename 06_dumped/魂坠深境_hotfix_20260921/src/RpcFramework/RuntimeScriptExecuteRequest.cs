namespace RpcFramework;

public sealed class RuntimeScriptExecuteRequest
{
	public string Body { get; set; } = string.Empty;

	public int TimeoutMs { get; set; }
}
