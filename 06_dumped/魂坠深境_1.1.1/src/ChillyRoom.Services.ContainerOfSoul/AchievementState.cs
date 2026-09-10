using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class AchievementState
{
	[JsonProperty("id", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string Id { get; set; }

	[JsonProperty("finished", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool Finished { get; set; }

	[JsonProperty("finishedLocalDate", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string FinishedLocalDate { get; set; }

	[JsonProperty("rewardGot", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool RewardGot { get; set; }
}
