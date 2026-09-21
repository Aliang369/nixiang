public class AddCharacterWillPowerTokenUpgrade : GameConfigEffect
{
	public int delta;

	public override void Prepare(CharacterGameInitConfig characterGameConfig)
	{
		characterGameConfig.initWillPower += delta;
	}

	public override string GenerateTalentDisplayDescription(out TextDataGenerator generator, PlayerCharacterState character = null, TalentDef def = null)
	{
		generator = null;
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_character_token_upgrade_add_willPower_arg0.FormatWith(delta);
	}

	public override string GenerateObsessionDisplayDescription(PlayerCharacterState character, CharacterObsessionLevelConfig obsessionConfig)
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_character_obsession_upgrade_unlock_chracter_arg0_add_will_power_arg1_with_append.FormatWith(character.Name, delta);
	}
}
