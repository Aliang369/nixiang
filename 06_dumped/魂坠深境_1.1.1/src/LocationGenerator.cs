using System;
using System.Collections.Generic;
using GameRandom;

public abstract class LocationGenerator
{
	public abstract PathLocation.LocationType LocationType { get; }

	public abstract EncounterSetupInfo GenerateRoomSetupInfo(GameStateInfo gameState, DungeonMap map, PathLocation loc);

	public virtual float GetSpawnGenerateWeight(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		return 0f;
	}

	public virtual bool CheckMustSpawn(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		return false;
	}

	public virtual bool CheckRejectSpawn(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		return false;
	}

	public static bool IsFightLocation(PathLocation.LocationType type)
	{
		if (type != PathLocation.LocationType.NormalFight && type != PathLocation.LocationType.EliteFight)
		{
			return type == PathLocation.LocationType.Boss;
		}
		return true;
	}

	public static RoomInfoData GetLocationRoomInMap(GameStateInfo gameState, DungeonMap map, PathLocation location)
	{
		DraftMap loadedDraftMap = map.GetLoadedDraftMap(gameState.applicationState.assetCatalog);
		RoomRef roomRef = loadedDraftMap.GetIndexPivot(location.draftMapIndex).randomRoomRefs.SelectRandom(gameState.randomManager.NextDouble);
		if (roomRef == null || !roomRef.IsValidRef(gameState.applicationState.assetCatalog))
		{
			roomRef = loadedDraftMap.GetAllValidRoomRefs(gameState.applicationState).SelectRandom(gameState.randomManager.NextDouble);
		}
		if (roomRef == null)
		{
			return gameState.applicationState.assetCatalog.roomInfoDataCatalog.assets[0];
		}
		return roomRef.GetAssetPreData(gameState.applicationState.assetCatalog);
	}

	protected static bool IsNearTo(DungeonMap map, PathLocation pivot, Func<PathLocation, bool> checker)
	{
		if (GetNeighbourLocations(map, pivot).Find((PathLocation t) => checker(t)) != null)
		{
			return true;
		}
		return false;
	}

	public static int GetContinuousConnectCount(DungeonMap map, PathLocation pivot, Func<PathLocation, bool> checker, HashSet<PathLocation> visited = null)
	{
		int num = 0;
		if (visited == null)
		{
			visited = new HashSet<PathLocation>();
		}
		if (visited.Contains(pivot))
		{
			return num;
		}
		visited.Add(pivot);
		foreach (PathLocation neighbourLocation in GetNeighbourLocations(map, pivot))
		{
			if (!visited.Contains(neighbourLocation) && checker(neighbourLocation))
			{
				num++;
				num += GetContinuousConnectCount(map, neighbourLocation, checker, visited);
			}
		}
		return num;
	}

	public static List<PathLocation> GetNeighbourLocations(DungeonMap map, PathLocation pivot)
	{
		List<PathLocation> list = new List<PathLocation>();
		list.AddRange(pivot.connections);
		foreach (PathLocation location in map.locations)
		{
			if (location.connections.Contains(pivot))
			{
				list.Add(location);
			}
		}
		return list;
	}
}
