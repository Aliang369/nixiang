using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[Preserve]
public class BossBlessStatisticItem
{
	[JsonProperty("blessId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public string BlessId { get; set; }

	[JsonProperty("exposureCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int ExposureCount { get; set; }

	[JsonProperty("pickCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int PickCount { get; set; }
}
