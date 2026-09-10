using Newtonsoft.Json.Linq;

namespace RpcFramework;

public sealed class AiWaitingInfo
{
	public string? Reason { get; set; }

	public string? Detail { get; set; }

	public bool ActionQueueBusy { get; set; }

	public string? BlockingUi { get; set; }

	public int AutoContinuedCount { get; set; }

	public JObject ToJObject()
	{
		return new JObject
		{
			["reason"] = Reason,
			["detail"] = Detail,
			["actionQueueBusy"] = ActionQueueBusy,
			["blockingUi"] = BlockingUi,
			["autoContinuedCount"] = AutoContinuedCount
		};
	}
}
