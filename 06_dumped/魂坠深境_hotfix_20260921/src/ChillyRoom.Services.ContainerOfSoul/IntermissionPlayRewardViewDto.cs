using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[Preserve]
public sealed class IntermissionPlayRewardViewDto
{
	[JsonProperty("type", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public string Type { get; set; }

	[JsonProperty("id", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public string Id { get; set; }

	[JsonProperty("count", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int Count { get; set; }
}
