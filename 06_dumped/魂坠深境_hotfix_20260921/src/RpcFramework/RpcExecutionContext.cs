using System;
using Newtonsoft.Json.Linq;

namespace RpcFramework;

public sealed class RpcExecutionContext
{
	private readonly Action<string, string?, string?, string?, JObject?> reportRunningStatus;

	public RpcExecutionContext(Action<string, string?, string?, string?, JObject?> reportRunningStatus)
	{
		this.reportRunningStatus = reportRunningStatus;
	}

	public void ReportPhase(string phase, string? state = "running", string? message = null, string? currentStep = null, JObject? details = null)
	{
		reportRunningStatus(phase, state, message, currentStep, details);
	}
}
