namespace RpcFramework;

public sealed class ClaimRequestResult
{
	public ClaimRequestState State { get; }

	public string? Path { get; }

	public static ClaimRequestResult None { get; } = new ClaimRequestResult(ClaimRequestState.None, null);

	private ClaimRequestResult(ClaimRequestState state, string? path)
	{
		State = state;
		Path = path;
	}

	public static ClaimRequestResult Claimed(string path)
	{
		return new ClaimRequestResult(ClaimRequestState.Claimed, path);
	}

	public static ClaimRequestResult DuplicateRejected(string resultPath)
	{
		return new ClaimRequestResult(ClaimRequestState.DuplicateRejected, resultPath);
	}
}
