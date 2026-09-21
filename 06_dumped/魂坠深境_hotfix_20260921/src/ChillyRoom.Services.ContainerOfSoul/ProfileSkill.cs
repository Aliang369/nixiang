using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class ProfileSkill
{
	[JsonProperty("guid", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string Guid { get; set; }

	[JsonProperty("skillDefId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string SkillDefId { get; set; }

	[JsonProperty("archetypeIndex", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int ArchetypeIndex { get; set; }

	[JsonProperty("hexShape", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public HexShapeSvr HexShape { get; set; }

	[JsonProperty("mountedSkillGems", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<SkillGem> MountedSkillGems { get; set; }

	[JsonProperty("isFavorite", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool IsFavorite { get; set; }
}
