public class AddCharacterCrystalTokenUpgrade : GameConfigEffect
{
	public int delta;

	public override void Prepare(CharacterGameInitConfig characterGameConfig)
	{
		characterGameConfig.initCrystal += delta;
	}

	public override string GenerateTalentDisplayDescription(out TextDataGenerator generator, PlayerCharacterState character = null, TalentDef def = null)
	{
		generator = null;
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_character_token_upgrade_add_crystal_arg0.FormatWith(delta);
	}
}
