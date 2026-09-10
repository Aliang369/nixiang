using System;
using System.Collections.Generic;

public class ItemGenerateGenerationInfo : BonusGenerationInfo
{
	public NewItemGenerator bonusGenerator;

	public override List<BattleBonusObject> GenerateBonusObject(GameStateInfo gameState)
	{
		new GameEvent(GameEvent.Type.Display, GameEvent.TriggerType.OutBattleDisplay).gameState = gameState;
		List<BattleBonusObject> list = new List<BattleBonusObject>();
		foreach (Item item in bonusGenerator.GenerateNewItems(GenerateDisplayEvent(gameState)) ?? throw new Exception("Generate item failed."))
		{
			if (item != null)
			{
				list.Add(new SingleItemBonus(item));
			}
		}
		return list;
	}
}
