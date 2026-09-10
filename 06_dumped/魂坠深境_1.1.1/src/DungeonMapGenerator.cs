using System.Collections.Generic;
using GameRandom;

public abstract class DungeonMapGenerator
{
	public abstract GameStateInfo GameState { get; }

	public abstract DungeonMap GenerateMap();

	protected PathLocation.LocationType GenerateRandomLocationType(PathLocation loc, DungeonMap map)
	{
		List<LocationGenerator> list = DungeonGenerationManager.GetAllLocationGeneratorInCurrentAssembly().FindAll((LocationGenerator l) => !l.CheckRejectSpawn(GameState, map, loc));
		foreach (LocationGenerator item in list)
		{
			if (item.CheckMustSpawn(GameState, map, loc))
			{
				return item.LocationType;
			}
		}
		if (list.Count == 0)
		{
			return PathLocation.LocationType.NormalFight;
		}
		return list.SelectRandomWeight((LocationGenerator lc) => lc.GetSpawnGenerateWeight(GameState, map, loc), GameState.randomManager.NextDouble).LocationType;
	}
}
