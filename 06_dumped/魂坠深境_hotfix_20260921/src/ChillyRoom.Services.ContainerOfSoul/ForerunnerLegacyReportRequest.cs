using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class ForerunnerLegacyReportRequest
{
	[JsonProperty("forerunnerLegacyGuid", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string ForerunnerLegacyGuid { get; set; }

	[JsonProperty("reportType", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public ForerunnerLegacyReportType ReportType { get; set; }

	[JsonProperty("commentNote", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string CommentNote { get; set; }
}
