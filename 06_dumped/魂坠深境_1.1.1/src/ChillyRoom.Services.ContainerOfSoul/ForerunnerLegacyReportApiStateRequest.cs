using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[Preserve]
public class ForerunnerLegacyReportApiStateRequest
{
	[JsonProperty("forerunnerLegacyGuid")]
	public string ForerunnerLegacyGuid { get; set; }
}
