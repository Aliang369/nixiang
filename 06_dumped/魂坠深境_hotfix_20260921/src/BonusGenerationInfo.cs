using System.Collections.Generic;

public abstract class BonusGenerationInfo
{
	public abstract List<BattleBonusObject> GenerateBonusObject(GameStateInfo gameState);

	protected GameEvent GenerateDisplayEvent(GameStateInfo gameState)
	{
		return new GameEvent(GameEvent.Type.Display, GameEvent.TriggerType.OutBattleDisplay)
		{
			gameState = gameState
		};
	}
}
