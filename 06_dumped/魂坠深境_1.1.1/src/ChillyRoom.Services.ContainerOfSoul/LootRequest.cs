using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class LootRequest
{
	[JsonProperty("matterId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string MatterId { get; set; }

	[JsonProperty("count", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int Count { get; set; }

	[JsonProperty("lootIndex", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int LootIndex { get; set; }

	public LootRequest(string matterId, int count, int lootIndex = -1)
	{
		MatterId = matterId;
		Count = count;
		LootIndex = lootIndex;
	}
}
