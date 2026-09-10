using System.Collections.Generic;

public class UnlockCharacterMastery : GameConfigEffect
{
	public CharacterMastery mastery;

	public override List<FeatureInfoData> GenerateFeatureInfoData(PlayerCharacterState character)
	{
		return new List<FeatureInfoData>(new FeatureInfoData[1]
		{
			new FeatureInfoData(mastery)
		});
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { mastery };
	}

	public override string GenerateObsessionDisplayDescription(PlayerCharacterState character, CharacterObsessionLevelConfig obsessionConfig)
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_character_obsession_upgrade_unlock_chracter_arg0_mastery_arg1_with_append.FormatWith(character.Name, mastery.masteryName);
	}
}
