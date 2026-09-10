using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[Preserve]
public class ForerunnerLegacyReportApiSubmitResult
{
	[JsonProperty("success")]
	public bool Success { get; set; }

	[JsonProperty("alreadySubmitted")]
	public bool AlreadySubmitted { get; set; }

	[JsonProperty("reportId")]
	public Guid? ReportId { get; set; }

	[JsonProperty("unavailableReason")]
	public ForerunnerLegacyReportApiUnavailableReason UnavailableReason { get; set; }
}
