using System.Collections.Generic;

public static class RunningGameSaveMigration
{
	private const string PainToLifeInfluenceId = "eaba925d4911e0866801b95480644283";

	public static bool Migrate(GameStateInfo gameState)
	{
		BattleFieldTransform battleFieldTransform = gameState?.playerState?.bft;
		if (battleFieldTransform == null)
		{
			return false;
		}
		return MigratePainToLifeMaxHealth(battleFieldTransform);
	}

	private static bool MigratePainToLifeMaxHealth(BattleFieldTransform bft)
	{
		Influence influence = bft.influences?.Find((Influence candidate) => candidate?.def?.id == "eaba925d4911e0866801b95480644283");
		if (influence == null)
		{
			return false;
		}
		Influence influence2 = influence;
		if (influence2.tempValues == null)
		{
			influence2.tempValues = new Dictionary<string, object>();
		}
		if (influence.tempValues.ContainsKey("PainToLife.MaxHealth50.V2"))
		{
			return false;
		}
		int num = (int)bft.maxHealth / 2;
		if (num > 0)
		{
			bft.maxHealth = CombatValueLimit.Clamp((long)(int)bft.maxHealth + (long)num);
			bft.health = CombatValueLimit.Clamp((long)(int)bft.health + (long)num);
		}
		influence.tempValues["PainToLife.MaxHealth50.V2"] = true;
		return true;
	}
}
