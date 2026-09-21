using System;

public class EncounterEnemyLevelValueGenerator : ValueGenerator
{
	public enum ValueType
	{
		Threat,
		Resistance
	}

	public ValueType type;

	public float mul;

	public override int? GenerateValue(GameEvent eve)
	{
		float num = 0f;
		if (eve.gameState?.currentEncounter == null)
		{
			return null;
		}
		num = type switch
		{
			ValueType.Threat => ScriptableObjectSingleton<GameStatisticData>.instance.GetThreatValue(EnemyEncounterType.Normal, eve.gameState.currentEncounter.enemyLevel, eve.gameState.difficultyState.contractDifficulty), 
			ValueType.Resistance => ScriptableObjectSingleton<GameStatisticData>.instance.GetResistanceValue(EnemyEncounterType.Normal, eve.gameState.currentEncounter.enemyLevel, eve.gameState.difficultyState.contractDifficulty), 
			_ => throw new ArgumentOutOfRangeException(), 
		} * mul;
		return (int)num;
	}
}
