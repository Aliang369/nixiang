using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class PlayerIndexBookData
{
	[JsonProperty("discoveredContent", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<string> DiscoveredContent { get; set; }

	[JsonProperty("discoveredRewardedContent", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<string> DiscoveredRewardedContent { get; set; }

	[JsonProperty("pageProgressRewardedLayers", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public Dictionary<IndexBookPage, int> PageProgressRewardedLayers { get; set; }
}
