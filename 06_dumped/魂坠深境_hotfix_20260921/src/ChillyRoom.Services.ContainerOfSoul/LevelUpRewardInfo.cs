using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class LevelUpRewardInfo
{
	[JsonProperty("levelIndex", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int LevelIndex { get; set; }

	[JsonProperty("baseReward", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public GlobalRewardInfo BaseReward { get; set; }

	[JsonProperty("appendReward", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public GlobalRewardInfo AppendReward { get; set; }
}
