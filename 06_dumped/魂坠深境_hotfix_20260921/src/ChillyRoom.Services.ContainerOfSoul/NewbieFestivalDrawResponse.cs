using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class NewbieFestivalDrawResponse
{
	[JsonProperty("activityId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string ActivityId { get; set; }

	[JsonProperty("drawnSlotIndex", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int DrawnSlotIndex { get; set; }

	[JsonProperty("reward", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public GlobalRewardInfo Reward { get; set; }

	[JsonProperty("drawnSlotIndexes", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<int> DrawnSlotIndexes { get; set; }
}
