namespace RpcFramework;

public sealed class RpcPolicyDecision
{
	public bool Allowed { get; set; }

	public string Phase { get; set; } = "UnityBusy";

	public string State { get; set; } = "running";

	public string? Message { get; set; }

	public static RpcPolicyDecision Allow { get; } = new RpcPolicyDecision
	{
		Allowed = true
	};
}
