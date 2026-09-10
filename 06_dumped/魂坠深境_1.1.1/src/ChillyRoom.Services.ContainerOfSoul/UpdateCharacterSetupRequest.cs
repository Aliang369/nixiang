using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class UpdateCharacterSetupRequest
{
	[JsonProperty("characterId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string CharacterId { get; set; }

	[JsonProperty("masteryId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string MasteryId { get; set; }

	[JsonProperty("skillPresetIndex", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int SkillPresetIndex { get; set; }

	[JsonProperty("skillPresets", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<List<string>> SkillPresets { get; set; }

	public UpdateCharacterSetupRequest(global::PlayerCharacterState character)
	{
		CharacterId = character.def.FullID;
		MasteryId = character.currentMastery?.FullID;
		SkillPresetIndex = character.skillPresets.IndexOf(character.skillPreset);
		SkillPresets = character.skillPresets.Select(ToIdSkillPreset).WhereNotNull().ToList();
	}

	public List<string> ToIdSkillPreset(ProfileSkillPreset preset)
	{
		List<string> list = new List<string>();
		foreach (PresetSkillSlot configSkillSlot in preset.configSkillSlots)
		{
			list.Add(configSkillSlot.skill?.profileSkillGUID);
		}
		return list;
	}
}
