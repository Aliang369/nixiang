public class AddCharacterActivateSkillSlot : GameConfigEffect
{
	public override void Prepare(CharacterGameInitConfig characterGameConfig)
	{
		characterGameConfig.activateSkillSlotCount++;
	}

	public override string GenerateObsessionDisplayDescription(PlayerCharacterState character, CharacterObsessionLevelConfig obsessionConfig)
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_character_obsession_upgrade_add_character_in_battle_activate_skill_count_by_one;
	}
}
