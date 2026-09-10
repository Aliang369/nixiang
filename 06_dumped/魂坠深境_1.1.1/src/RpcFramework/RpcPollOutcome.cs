namespace RpcFramework;

public sealed class RpcPollOutcome
{
	public RpcPollOutcomeKind Kind { get; }

	public RpcResult? Result { get; }

	public string? Path { get; }

	private RpcPollOutcome(RpcPollOutcomeKind kind, RpcResult? result = null, string? path = null)
	{
		Kind = kind;
		Result = result;
		Path = path;
	}

	public static RpcPollOutcome NoRequest()
	{
		return new RpcPollOutcome(RpcPollOutcomeKind.NoRequest);
	}

	public static RpcPollOutcome Busy(string requestPath)
	{
		return new RpcPollOutcome(RpcPollOutcomeKind.Busy, null, requestPath);
	}

	public static RpcPollOutcome Completed(RpcResult result)
	{
		return new RpcPollOutcome(RpcPollOutcomeKind.Completed, result);
	}

	public static RpcPollOutcome DuplicateRejected(string resultPath)
	{
		return new RpcPollOutcome(RpcPollOutcomeKind.DuplicateRejected, null, resultPath);
	}
}
