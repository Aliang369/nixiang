using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class TriggerDailyCheckInResult
{
	[JsonProperty("monthDailyCheckInRewards", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public MonthCheckInResult MonthDailyCheckInRewards { get; set; }

	[JsonProperty("monthlyCardCheckInResult", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public MonthCardCheckInResult MonthlyCardCheckInResult { get; set; }

	[JsonProperty("festivalCheckInResult", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public FestivalCheckInResult FestivalCheckInResult { get; set; }

	[JsonProperty("forerunnerLegacyRewardResult", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public GetLegacyShareRewardResult ForerunnerLegacyRewardResult { get; set; }
}
