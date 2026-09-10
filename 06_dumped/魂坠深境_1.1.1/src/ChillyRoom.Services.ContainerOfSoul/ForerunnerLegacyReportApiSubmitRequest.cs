using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[Preserve]
public class ForerunnerLegacyReportApiSubmitRequest
{
	[JsonProperty("forerunnerLegacyGuid")]
	public string ForerunnerLegacyGuid { get; set; }

	[JsonProperty("reportType")]
	public ForerunnerLegacyReportApiType ReportType { get; set; }

	[JsonProperty("commentNote")]
	public string CommentNote { get; set; }
}
