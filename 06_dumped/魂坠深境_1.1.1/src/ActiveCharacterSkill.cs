public class ActiveCharacterSkill : GameConfigEffect
{
	public override string GenerateTalentDisplayDescription(out TextDataGenerator generator, PlayerCharacterState character = null, TalentDef def = null)
	{
		GameEvent gameEvent = new GameEvent(GameEvent.Type.Display, GameEvent.TriggerType.OutBattleDisplay);
		gameEvent.gameState = TextDataGenerator.GenerateGameState(base.applicationState);
		gameEvent.nodeTempValues.Add("character", character);
		return TextDataPreset.FormatEffectObjectDescription(def, def.effectDescription, gameEvent, out generator);
	}
}
