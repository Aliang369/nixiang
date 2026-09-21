using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

public class DraftDungeonMapGenerator : DungeonMapGenerator
{
	public class DraftMapGenerationConfig
	{
		public GameStateInfo gameState;

		public int baseGlobalProgress;

		public int baseStageProgress;

		public int mapMaxDifficulty;

		public int mapMinDifficulty;

		public int stage;

		public DraftMapInfoData draftMap;

		public Level level;

		public FixedContractRouteDef.StageRoute fixedStageRoute;
	}

	public DraftMapGenerationConfig config;

	public override GameStateInfo GameState => config.gameState;

	public DraftDungeonMapGenerator(DraftMapGenerationConfig config)
	{
		this.config = config;
	}

	public override DungeonMap GenerateMap()
	{
		DraftMap orSyncLoadAsset = config.draftMap.GetOrSyncLoadAsset();
		List<DraftPivot> allVisiblePivots = orSyncLoadAsset.GetAllVisiblePivots();
		Dictionary<DraftPivot, PathLocation> dictionary = new Dictionary<DraftPivot, PathLocation>();
		List<PathConnectionLine> list = new List<PathConnectionLine>();
		List<PathLocation> list2 = new List<PathLocation>();
		DungeonMap dungeonMap = new DungeonMap();
		dungeonMap.locations = list2;
		dungeonMap.connectionLines = list;
		dungeonMap.mapSize = orSyncLoadAsset.GetComponent<RectTransform>().sizeDelta;
		dungeonMap.sourceDraftMap = new DraftMapRef(config.draftMap);
		foreach (DraftPivot item in allVisiblePivots)
		{
			PathLocation pathLocation = new PathLocation(GameState);
			pathLocation.localProgress = item.localProgress;
			pathLocation.stageProgress = config.baseStageProgress + item.localProgress;
			pathLocation.globalProgress = config.baseGlobalProgress + item.localProgress;
			pathLocation.displayPosition = item.transform.localPosition;
			pathLocation.draftMapIndex = orSyncLoadAsset.GetPivotIndex(item);
			dictionary.Add(item, pathLocation);
			list2.Add(pathLocation);
		}
		foreach (DraftPivot item2 in allVisiblePivots)
		{
			foreach (DraftPivot.PivotConnection validConnection in item2.ValidConnections)
			{
				if (dictionary.ContainsKey(validConnection.target))
				{
					dictionary[item2].connections.Add(dictionary[validConnection.target]);
					PathConnectionLine pathConnectionLine = new PathConnectionLine();
					pathConnectionLine.from = dictionary[item2];
					pathConnectionLine.to = dictionary[validConnection.target];
					pathConnectionLine.controlPoints = validConnection.GetAllControlPoints(item2);
					list.Add(pathConnectionLine);
				}
			}
		}
		FixedContractRouteDef.StageRoute fixedStageRoute = config.fixedStageRoute;
		if (fixedStageRoute == null || !fixedStageRoute.HasFixedNodes)
		{
			SetForerunnerLocationType(dictionary);
		}
		foreach (DraftPivot item3 in allVisiblePivots.Shuffle(GameState.randomManager.NextDouble))
		{
			if (dictionary[item3].type != PathLocation.LocationType.None)
			{
				continue;
			}
			FixedContractRouteDef.StageRoute fixedStageRoute2 = config.fixedStageRoute;
			if (fixedStageRoute2 != null && fixedStageRoute2.HasFixedNodes)
			{
				ApplyFixedNodeType(orSyncLoadAsset, item3, dictionary[item3]);
				continue;
			}
			switch (item3.type)
			{
			case DraftPivot.PivotType.Random:
				dictionary[item3].type = GenerateRandomLocationType(dictionary[item3], dungeonMap);
				break;
			case DraftPivot.PivotType.SectionStart:
				dictionary[item3].type = PathLocation.LocationType.SectionStart;
				break;
			case DraftPivot.PivotType.Boss:
				dictionary[item3].type = PathLocation.LocationType.Boss;
				dictionary[item3].isNodePoint = true;
				break;
			case DraftPivot.PivotType.SectionEnd:
				dictionary[item3].type = PathLocation.LocationType.SectionEnd;
				dictionary[item3].isNodePoint = true;
				break;
			case DraftPivot.PivotType.Heal:
				dictionary[item3].type = PathLocation.LocationType.HealFire;
				break;
			case DraftPivot.PivotType.Custom:
				dictionary[item3].isCustomSpeicalLocation = true;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		dungeonMap.initLocation = list2.Where((PathLocation l) => l.type == PathLocation.LocationType.SectionStart).ToList().SelectRandom(GameState.randomManager.NextDouble);
		float num = list2.First((PathLocation l) => l.type == PathLocation.LocationType.Boss || l.type == PathLocation.LocationType.SectionEnd || l.type == PathLocation.LocationType.PVPEndPoint).localProgress;
		foreach (PathLocation item4 in list2)
		{
			if (item4.type == PathLocation.LocationType.Boss)
			{
				item4.enemyDifficultyLevel = config.mapMaxDifficulty;
				continue;
			}
			float t = (float)item4.localProgress / num;
			item4.enemyDifficultyLevel = Mathf.FloorToInt(Mathf.Lerp(config.mapMinDifficulty, (float)config.mapMaxDifficulty + 0.999f, t));
			item4.enemyDifficultyLevel = Mathf.Clamp(item4.enemyDifficultyLevel, config.mapMinDifficulty, config.mapMaxDifficulty);
		}
		foreach (DraftPivot key in dictionary.Keys)
		{
			if (key.beforeModifier != null)
			{
				key.beforeModifier.ModifyLocation(dictionary[key], key, dungeonMap, GameState);
			}
		}
		ApplyFixedNodeTypes(orSyncLoadAsset, dictionary);
		foreach (PathLocation item5 in list2)
		{
			LocationGenerator locationGenerator = ScriptableObjectSingleton<DungeonGenerationManager>.instance.GetLocationGenerator(item5.type);
			if (locationGenerator != null && item5.attachEncounterSetupInfo == null)
			{
				item5.attachEncounterSetupInfo = locationGenerator.GenerateRoomSetupInfo(GameState, dungeonMap, item5);
			}
		}
		GenerateLocationModifier(list2, dictionary);
		foreach (DraftPivot key2 in dictionary.Keys)
		{
			if (key2.afterModifier != null)
			{
				key2.afterModifier.ModifyLocation(dictionary[key2], key2, dungeonMap, GameState);
			}
		}
		ApplyFixedNodeContents(orSyncLoadAsset, dictionary);
		ApplyPresetDisorderCriticalInfluences(dictionary);
		GenerateDisorderCriticalModifiers(list2);
		PreselectBossEnemy(dungeonMap);
		return dungeonMap;
	}

	private void ApplyFixedNodeTypes(DraftMap draftMap, Dictionary<DraftPivot, PathLocation> locations)
	{
		FixedContractRouteDef.StageRoute fixedStageRoute = config.fixedStageRoute;
		if (fixedStageRoute == null || !fixedStageRoute.HasFixedNodes)
		{
			return;
		}
		foreach (KeyValuePair<DraftPivot, PathLocation> location in locations)
		{
			ApplyFixedNodeType(draftMap, location.Key, location.Value);
		}
	}

	private void ApplyFixedNodeType(DraftMap draftMap, DraftPivot pivot, PathLocation location)
	{
		int pivotIndex = draftMap.GetPivotIndex(pivot);
		FixedContractRouteDef.FixedNode node = config.fixedStageRoute.GetNode(pivotIndex);
		if (node == null)
		{
			throw new InvalidOperationException($"Fixed route stage {config.fixedStageRoute.stage} is missing map node {pivotIndex}.");
		}
		location.type = node.locationType;
		location.isNodePoint = location.type == PathLocation.LocationType.Boss || location.type == PathLocation.LocationType.SectionEnd;
	}

	private void ApplyFixedNodeContents(DraftMap draftMap, Dictionary<DraftPivot, PathLocation> locations)
	{
		FixedContractRouteDef.StageRoute fixedStageRoute = config.fixedStageRoute;
		if (fixedStageRoute == null || !fixedStageRoute.HasFixedNodes)
		{
			return;
		}
		foreach (KeyValuePair<DraftPivot, PathLocation> location in locations)
		{
			int pivotIndex = draftMap.GetPivotIndex(location.Key);
			FixedContractRouteDef.FixedNode node = config.fixedStageRoute.GetNode(pivotIndex);
			RoomInfoData roomInfoData = node?.room?.GetAssetPreData(GameState.applicationState.assetCatalog);
			if (node == null || roomInfoData == null)
			{
				throw new InvalidOperationException($"Fixed route stage {config.fixedStageRoute.stage} node {pivotIndex} has no valid room.");
			}
			PathLocation value = location.Value;
			value.type = node.locationType;
			PathLocation pathLocation = value;
			if (pathLocation.attachEncounterSetupInfo == null)
			{
				pathLocation.attachEncounterSetupInfo = new EncounterSetupInfo
				{
					level = config.level,
					enemyLevel = value.enemyDifficultyLevel
				};
			}
			value.attachEncounterSetupInfo.roomInfoData = roomInfoData;
		}
	}

	private void PreselectBossEnemy(DungeonMap map)
	{
		WorldEvent worldEvent = map?.initLocation?.attachEncounterSetupInfo?.fixedEventTemplate;
		RiftWorldEvent riftWorldEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.riftWorldEventTemplate;
		if (!(riftWorldEventTemplate == null) && !(worldEvent != riftWorldEventTemplate))
		{
			PathLocation visibleEndLocation = map.VisibleEndLocation;
			if (visibleEndLocation != null && visibleEndLocation.type == PathLocation.LocationType.Boss && !GameState.generationManager.TryPreselectEnemyForEncounter(GameState, visibleEndLocation.attachEncounterSetupInfo, visibleEndLocation))
			{
				TraceLog.Warn($"未能为Boss地点预选敌人，stage={config.stage}, progress={visibleEndLocation.localProgress}");
			}
		}
	}

	private void GenerateDisorderCriticalModifiers(IEnumerable<PathLocation> locations)
	{
		if (GameState.contractState.selectedContract.difficultyLevel < DifficultyLevel.NightmareD7)
		{
			return;
		}
		List<InfluenceDef> list = ScriptableObjectSingleton<GameStatisticData>.instance?.nightmareTable?.Columns?.OfType<InfluenceDef>().ToList();
		if (list.IsNullOrEmpty())
		{
			return;
		}
		foreach (PathLocation item in locations.Where((PathLocation location) => location.IsFightLocation && location.disorderCriticalInfluence == null))
		{
			item.disorderCriticalInfluence = list.SelectRandom(GameState.randomManager.NextDouble);
		}
	}

	private static void ApplyPresetDisorderCriticalInfluences(Dictionary<DraftPivot, PathLocation> locations)
	{
		foreach (KeyValuePair<DraftPivot, PathLocation> location in locations)
		{
			if (location.Key.presetDisorderCriticalInfluence != null && location.Value.IsFightLocation)
			{
				location.Value.disorderCriticalInfluence = location.Key.presetDisorderCriticalInfluence;
			}
		}
	}

	private void GenerateLocationModifier(List<PathLocation> locations, Dictionary<DraftPivot, PathLocation> draftLocDic)
	{
		foreach (KeyValuePair<DraftPivot, PathLocation> item in draftLocDic)
		{
			DraftPivot key = item.Key;
			PathLocation value = item.Value;
			if (key.presetEnvironmentInfluence != null && value.IsFightLocation)
			{
				value.locationEnvironmentInfluence = key.presetEnvironmentInfluence;
			}
		}
		if (!GameState.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableRandomEnvironmentInfluenceAndLocationVariant))
		{
			return;
		}
		List<PathLocation> list = locations.Where((PathLocation l) => !l.isCustomSpeicalLocation && l.locationEnvironmentInfluence == null && l.IsFightLocation).ToList();
		int count = Mathf.RoundToInt((float)list.Count / ScriptableObjectSingleton<DungeonGenerationManager>.instance.pathLocationModifierDisplayConfig.locationPerRandomModifierCount);
		foreach (PathLocation item2 in list.SelectRandomCountUnique(count, GameState.randomManager.NextDouble))
		{
			EnvironmentInfluenceDef locationEnvironmentInfluence = GameState.generationManager.GenerateEnvironmentInfluenceDef(config.level, item2);
			item2.locationEnvironmentInfluence = locationEnvironmentInfluence;
		}
		List<PathLocation> list2 = locations.Where((PathLocation l) => !l.isCustomSpeicalLocation && l.variantType == PathLocation.LocationVariantType.None).ToList();
		int count2 = Mathf.RoundToInt((float)list2.Count / ScriptableObjectSingleton<DungeonGenerationManager>.instance.pathLocationModifierDisplayConfig.locationPerRandomVairantCount);
		List<(PathLocation.LocationVariantType, float)> list3 = new List<(PathLocation.LocationVariantType, float)>();
		list3.AddRange(ScriptableObjectSingleton<DungeonGenerationManager>.instance.pathLocationModifierDisplayConfig.randomLocationVariantTypeWeightPool);
		if (GameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.EnableHardLocationVariant))
		{
			list3.AddRange(ScriptableObjectSingleton<DungeonGenerationManager>.instance.pathLocationModifierDisplayConfig.hardRandomLocationVariantTypeWeightPool);
		}
		foreach (PathLocation.LocationVariantType variant in (from t in list3.SelectRandomWeightCount(((PathLocation.LocationVariantType, float) p) => p.Item2, count2, GameState.randomManager.NextDouble)
			select t.Item1).ToList())
		{
			List<PathLocation> list4 = list2.Where((PathLocation loc) => PathLocation.CheckLocationVariantRandomGenerateAbleToLoc(variant, loc, GameState)).ToList();
			if (list4.Any())
			{
				list4.SelectRandom(GameState.randomManager.NextDouble).variantType = variant;
			}
		}
	}

	private void SetForerunnerLocationType(Dictionary<DraftPivot, PathLocation> draftLocDic)
	{
		if (GameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.DisableForerunnerLegacySpawn))
		{
			return;
		}
		List<ForerunnerLegacy> list = GameState.contractState.forerunnerLegacies.FindAll((ForerunnerLegacy l) => l.sourceDraftMap?.CTAssetId == config.draftMap?.FullId);
		if (list.IsNullOrEmpty())
		{
			return;
		}
		foreach (ForerunnerLegacy item in list)
		{
			List<KeyValuePair<DraftPivot, PathLocation>> source = draftLocDic.Where((KeyValuePair<DraftPivot, PathLocation> kv) => kv.Key.type == DraftPivot.PivotType.Random && kv.Value.forerunnerLegacy == null).ToList();
			if (!source.Any())
			{
				break;
			}
			PathLocation value = source.ToList().SelectRandom(GameState.randomManager.NextDouble).Value;
			value.type = PathLocation.LocationType.ForerunnerLegacy;
			value.forerunnerLegacy = item;
		}
	}
}
