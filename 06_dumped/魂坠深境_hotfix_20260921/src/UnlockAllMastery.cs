using System.Collections.Generic;
using System.Linq;

public class UnlockAllMastery : GameConfigEffect
{
	public override List<FeatureInfoData> GenerateFeatureInfoData(PlayerCharacterState character)
	{
		return character.def.characterMasteries.Select((CharacterMastery m) => new FeatureInfoData(m)).ToList();
	}

	public override string GenerateObsessionDisplayDescription(PlayerCharacterState character, CharacterObsessionLevelConfig obsessionConfig)
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_character_obsession_upgrade_unlock_chracter_all_mastery_arg0_with_append.FormatWith(character.Name);
	}
}
