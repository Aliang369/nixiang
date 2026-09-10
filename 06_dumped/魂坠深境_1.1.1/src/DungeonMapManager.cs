using System.Collections;
using System.Collections.Generic;

public class DungeonMapManager : InGameManager
{
	[SaveField(null, null)]
	public DungeonMap currentDungeonMap;

	[SaveField(null, null)]
	public int currentGlobalProgress;

	[SaveField(null, null)]
	private new GameStateInfo gameState;

	[SaveField(null, null)]
	public bool inMapMode;

	[SaveField(null, null)]
	public Level currentLevel;

	[SaveField(null, null)]
	public List<DraftMapRef> visitedDraftMapRefs;

	public PathLocation currentLocation => currentDungeonMap?.currentLocation;

	public int currentLocalProgress => currentLocation?.localProgress ?? (-1);

	public override void Init(GameStateInfo gameState)
	{
		this.gameState = gameState;
		if (visitedDraftMapRefs == null)
		{
			visitedDraftMapRefs = new List<DraftMapRef>();
		}
		TrackKnownDraftMap(currentDungeonMap?.sourceDraftMap);
	}

	public void SetDungeonMap(DungeonMap map)
	{
		DraftMapRef previousDraftMapRef = currentDungeonMap?.sourceDraftMap;
		currentDungeonMap = map;
		TrackKnownDraftMap(map?.sourceDraftMap);
		gameState.UIManager.dungeonMapPanel.Setup(currentDungeonMap);
		gameState.UIManager.dungeonMapPanel_Preview.Setup(currentDungeonMap);
		ReleasePreviousDraftMapAfterViewSwitch(previousDraftMapRef);
		foreach (PathLocation location in gameState.dungeonMapManager.currentDungeonMap.locations)
		{
			ThinkingAnalyticsExtends.TrackExposeLocation(gameState, location);
		}
	}

	public bool CanReach(PathLocation location)
	{
		if (currentLocation == null)
		{
			return true;
		}
		return currentLocation.CanReach(location);
	}

	public bool CanJump(PathLocation location)
	{
		if (currentLocation == null)
		{
			return location.localProgress == 0;
		}
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.NoPathForkLimit) && location.localProgress == currentLocation.localProgress + 1)
		{
			return true;
		}
		if (CanJumpByProgressDistance(currentLocation, location))
		{
			return true;
		}
		return currentLocation.connections.Contains(location);
	}

	public bool CanJumpByProgressDistance(PathLocation from, PathLocation to)
	{
		if (from == null || to == null)
		{
			return false;
		}
		int variableValue = gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.MapJumpProgressDistanceDelta);
		if (variableValue <= 0)
		{
			return false;
		}
		int num = to.localProgress - from.localProgress;
		if (num > 1)
		{
			return num <= 1 + variableValue;
		}
		return false;
	}

	private void TrackKnownDraftMap(DraftMapRef draftMapRef)
	{
		if (draftMapRef != null)
		{
			if (visitedDraftMapRefs == null)
			{
				visitedDraftMapRefs = new List<DraftMapRef>();
			}
			object obj;
			if (visitedDraftMapRefs.Count <= 0)
			{
				obj = null;
			}
			else
			{
				List<DraftMapRef> list = visitedDraftMapRefs;
				obj = list[list.Count - 1];
			}
			if (!(((AssetPreDataRef<DraftMapInfoData, DraftMap>)obj)?.CTAssetId == draftMapRef.CTAssetId))
			{
				visitedDraftMapRefs.Add(draftMapRef);
			}
		}
	}

	private void ReleasePreviousDraftMapAfterViewSwitch(DraftMapRef previousDraftMapRef)
	{
		if (previousDraftMapRef != null && !(previousDraftMapRef.CTAssetId == currentDungeonMap?.sourceDraftMap?.CTAssetId))
		{
			gameState.StartCoroutine(ReleaseCor);
		}
		IEnumerator ReleaseCor()
		{
			yield return null;
			if (!(previousDraftMapRef.CTAssetId == currentDungeonMap?.sourceDraftMap?.CTAssetId))
			{
				AssetCatalog assetCatalog = gameState.applicationState.assetCatalog;
				if (previousDraftMapRef.IsLoaded(assetCatalog))
				{
					previousDraftMapRef.Unload(assetCatalog);
					TraceLog.Debug("释放旧地图Prefab: " + previousDraftMapRef.CTAssetId);
				}
			}
		}
	}
}
