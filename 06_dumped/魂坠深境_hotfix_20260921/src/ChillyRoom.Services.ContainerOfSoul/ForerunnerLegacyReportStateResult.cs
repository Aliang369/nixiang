using System;
using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class ForerunnerLegacyReportStateResult
{
	[JsonProperty("canReport", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool CanReport { get; set; }

	[JsonProperty("alreadySubmitted", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool AlreadySubmitted { get; set; }

	[JsonProperty("reportId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public Guid? ReportId { get; set; }

	[JsonProperty("unavailableReason", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public ForerunnerLegacyReportUnavailableReason UnavailableReason { get; set; }
}
