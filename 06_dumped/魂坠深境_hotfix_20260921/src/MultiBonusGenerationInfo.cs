using System.Collections.Generic;

public class MultiBonusGenerationInfo : BonusGenerationInfo
{
	public List<BonusGenerationInfo> generators;

	public override List<BattleBonusObject> GenerateBonusObject(GameStateInfo gameState)
	{
		if (generators == null)
		{
			return null;
		}
		List<BattleBonusObject> list = new List<BattleBonusObject>();
		foreach (BonusGenerationInfo generator in generators)
		{
			list.AddRange(generator.GenerateBonusObject(gameState));
		}
		return list;
	}
}
