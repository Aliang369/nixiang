using System.Collections.Generic;

public class CurrentStanceDefSelector : StanceDefSelector
{
	public override List<StanceDef> GenerateStanceDefs(GameEvent eve)
	{
		if (eve.gameState == null || eve.gameState.stanceManager == null || eve.gameState.stanceManager.currentStance == null)
		{
			return null;
		}
		Stance currentStance = eve.gameState.stanceManager.currentStance;
		if (currentStance.def == null)
		{
			return null;
		}
		return new List<StanceDef> { currentStance.def };
	}

	public override StanceDef GenerateSingleStanceDef(GameEvent eve)
	{
		return eve.gameState?.stanceManager?.currentStance?.def;
	}
}
