using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class RecycleObsessionResult
{
	[JsonProperty("gainMatters", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<MatterInfo> GainMatters { get; set; }

	[JsonProperty("consumedMatters", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<MatterInfo> ConsumedMatters { get; set; }

	[JsonProperty("quota", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public ObsessionDebrisRecycleQuota Quota { get; set; }
}
