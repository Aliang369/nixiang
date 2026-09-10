using UnityEngine;

public class NormalFightLocationGenerator : LocationGenerator
{
	public override PathLocation.LocationType LocationType => PathLocation.LocationType.NormalFight;

	public override float GetSpawnGenerateWeight(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		float num = 100f;
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.GenerateMoreFightLocation))
		{
			num = 125f;
		}
		float num2 = 1f + Mathf.Clamp(0.5f - (float)(gameState.contractState.stage - 1) / 5f, -1f, 1f);
		num *= num2;
		if (LocationGenerator.IsNearTo(map, insertLoc, (PathLocation l) => LocationGenerator.IsFightLocation(l.type)))
		{
			num /= 2f;
		}
		return num;
	}

	public override bool CheckRejectSpawn(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		if (LocationGenerator.GetContinuousConnectCount(map, insertLoc, (PathLocation l) => LocationGenerator.IsFightLocation(l.type)) >= 2)
		{
			return true;
		}
		return base.CheckRejectSpawn(gameState, map, insertLoc);
	}

	public override EncounterSetupInfo GenerateRoomSetupInfo(GameStateInfo gameState, DungeonMap map, PathLocation loc)
	{
		Level currentLevel = gameState.contractState.currentLevel;
		return new EncounterSetupInfo
		{
			enemySpawnType = EnemyEncounterType.Normal,
			level = currentLevel,
			roomInfoData = LocationGenerator.GetLocationRoomInMap(gameState, map, loc),
			fixedEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.enterBattleEventTemplate,
			enemyLevel = loc.enemyDifficultyLevel
		};
	}
}
