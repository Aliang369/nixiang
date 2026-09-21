using System.Collections.Generic;

public class HealAllBonusGenerationInfo : BonusGenerationInfo
{
	public override List<BattleBonusObject> GenerateBonusObject(GameStateInfo gameState)
	{
		return new List<BattleBonusObject>
		{
			new HealAllBonus()
		};
	}
}
