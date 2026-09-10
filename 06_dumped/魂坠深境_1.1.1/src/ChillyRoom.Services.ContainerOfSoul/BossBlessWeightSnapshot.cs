using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[Preserve]
public class BossBlessWeightSnapshot
{
	[JsonProperty("statDate", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int StatDate { get; set; }

	[JsonProperty("weights", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<string, double> Weights { get; set; }
}
