public class AddCharacterWillPower : GameConfigEffect
{
	public int delta;

	public override void Prepare(CharacterGameInitConfig characterGameConfig)
	{
		characterGameConfig.initWillPower += delta;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(delta)
		};
	}

	public override string GenerateObsessionDisplayDescription(PlayerCharacterState character, CharacterObsessionLevelConfig obsessionConfig)
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_character_obsession_upgrade_unlock_chracter_arg0_add_will_power_arg1_with_append.FormatWith(character.Name, delta);
	}
}
