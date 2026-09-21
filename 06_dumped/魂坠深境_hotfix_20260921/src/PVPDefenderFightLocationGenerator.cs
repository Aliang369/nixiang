using System.Collections.Generic;
using System.Linq;
using GameRandom;

public class PVPDefenderFightLocationGenerator : LocationGenerator
{
	public override PathLocation.LocationType LocationType => PathLocation.LocationType.PVPDefenderFight;

	public override bool CheckRejectSpawn(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		return true;
	}

	public override EncounterSetupInfo GenerateRoomSetupInfo(GameStateInfo gameState, DungeonMap map, PathLocation loc)
	{
		int index = map.locations.Where((PathLocation l) => l.type == PathLocation.LocationType.PVPDefenderFight).ToList().IndexOf(loc);
		PVPDefender pVPDefender = gameState.newGameConfigData.startGameInfo.pvpDefenderThisGame[index];
		return new EncounterSetupInfo
		{
			enemySpawnType = EnemyEncounterType.None,
			level = gameState.contractState.currentLevel,
			enemyLevel = loc.enemyDifficultyLevel,
			roomInfoData = LocationGenerator.GetLocationRoomInMap(gameState, map, loc),
			fixedEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.enterBattleEventTemplate,
			pvpDefender = pVPDefender,
			roomBonus = GeneratePvpDefenderBonus(pVPDefender)
		};
	}

	public BattleBonus GeneratePvpDefenderBonus(PVPDefender defender)
	{
		List<Item> items = defender.deckPreset.GetAllItems().SelectRandomCountUnique(3);
		return new BattleBonus
		{
			bonuses = 
			{
				(BattleBonusObject)new SelectItemFromItemsBonus(items),
				(BattleBonusObject)new MatterBonus(new MatterInfo(ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpBonusMatter, ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpMatterBonusPerChallengeBattleWin))
			}
		};
	}
}
