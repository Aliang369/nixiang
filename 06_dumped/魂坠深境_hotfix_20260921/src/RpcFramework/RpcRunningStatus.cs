using System;
using Newtonsoft.Json.Linq;

namespace RpcFramework;

public sealed class RpcRunningStatus
{
	public string JobId { get; set; } = string.Empty;

	public string? State { get; set; }

	public string? Action { get; set; }

	public string? Phase { get; set; }

	public string? CurrentStep { get; set; }

	public string? Message { get; set; }

	public DateTimeOffset StartedAt { get; set; }

	public DateTimeOffset UpdatedAt { get; set; }

	public JObject? Details { get; set; }

	public JObject ToJObject()
	{
		return new JObject
		{
			["jobId"] = JobId,
			["state"] = State,
			["action"] = Action,
			["phase"] = Phase,
			["currentStep"] = CurrentStep,
			["message"] = Message,
			["startedAt"] = StartedAt.ToString("o"),
			["updatedAt"] = UpdatedAt.ToString("o"),
			["details"] = Details
		};
	}
}
