using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class CardRecord
{
	[JsonProperty("id", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string Id { get; set; }

	[JsonProperty("cardSkinId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string CardSkinId { get; set; }

	[JsonProperty("level", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int Level { get; set; }

	[JsonProperty("counter", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int Counter { get; set; }

	[JsonProperty("maxCounter", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int MaxCounter { get; set; }

	[JsonProperty("runes", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<RuneRecord> Runes { get; set; }
}
