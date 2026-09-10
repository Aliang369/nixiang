using System.Collections.Generic;
using UnityEngine;

public class MarkSharePotential : GameConfigEffect
{
	public override List<FeatureInfoData> GenerateFeatureInfoData(PlayerCharacterState character)
	{
		return new List<FeatureInfoData>(new FeatureInfoData[1]
		{
			new FeatureInfoData(character.def.sharePotential)
		});
	}

	public override Sprite GenerateTalentDisplayIcon(PlayerCharacterState character, TalentDef def)
	{
		return character.def.sharePotential.icon;
	}

	public override string GenerateTalentDisplayName(PlayerCharacterState character, TalentDef def)
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_talent_share_potential_arg0.FormatWith(character?.def.sharePotential.GetDisplayName(base.applicationState, character) ?? "");
	}

	public override string GenerateObsessionDisplayDescription(PlayerCharacterState character, CharacterObsessionLevelConfig obsessionConfig)
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_character_obsession_upgrade_with_share_potential_arg0_with_append.FormatWith(character?.def.sharePotential?.GetDisplayName(base.applicationState, character));
	}
}
