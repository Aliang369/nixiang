using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class SkillRecord
{
	[JsonProperty("id", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string Id { get; set; }

	[JsonProperty("skillArchetypeIndex", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int SkillArchetypeIndex { get; set; }

	[JsonProperty("durability", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int Durability { get; set; }

	[JsonProperty("activated", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool Activated { get; set; }

	[JsonProperty("consumed", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool Consumed { get; set; }

	[JsonProperty("hexShape", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public HexShapeSvr HexShape { get; set; }

	[JsonProperty("skillGems", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<SkillGem> SkillGems { get; set; }
}
