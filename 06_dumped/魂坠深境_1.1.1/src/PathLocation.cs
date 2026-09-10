using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathLocation
{
	public enum LocationType
	{
		None = 0,
		NormalFight = 1,
		EliteFight = 2,
		Event = 10,
		BonusGame = 11,
		Ascension = 12,
		Boss = 30,
		EndCity = 40,
		HealFire = 50,
		GoldStore = 51,
		UpgradeRoom = 52,
		AttachRune = 53,
		CrystalUpgradeRoom = 61,
		ForerunnerLegacy = 62,
		SectionStart = 70,
		SectionEnd = 71,
		PVPDefenderFight = 2001,
		PVPEndPoint = 2002
	}

	public enum LocationVariantType
	{
		None,
		CrystalUpgradeFreeBless,
		EliteFightHighLevelEnemy,
		UpgradeRoomFreeInitUpgrade,
		GoldStoreMoreItem,
		AttachRuneDoubleRune,
		HealFireFullHeal,
		GoldStoreEndlessRefresh,
		CrystalUpgradeEndlessRefreshAndGet,
		AttachRuneEndlessRegenerate,
		FinalBoss,
		FinalEndingHealFire,
		FadedHealFire,
		HealthCostUpgradeRoom,
		EnhanceRune,
		BigBonusLocation,
		AmbushFight,
		EliteVariant
	}

	[SaveField(null, null)]
	private readonly GameStateInfo gameState;

	[SaveField(null, null)]
	public int localProgress;

	[SaveField(null, null)]
	public int stageProgress;

	[SaveField(null, null)]
	public int globalProgress;

	[SaveField(null, null)]
	public int enemyDifficultyLevel;

	[SaveField(null, null)]
	public bool finished;

	[SaveField(null, null)]
	public bool visited;

	[SaveField(null, null)]
	public bool spawnDisplay = true;

	[SaveField(null, null)]
	public bool hideLoc;

	[SaveField(null, null)]
	public LocationType type;

	[SaveField(null, null)]
	public LocationVariantType variantType;

	[SaveField(null, null)]
	public List<PathLocation> connections;

	[SaveField(null, null)]
	public bool isNodePoint;

	[SaveField(null, null)]
	public EncounterSetupInfo attachEncounterSetupInfo;

	[SaveField(null, null)]
	public Vector2 displayPosition;

	[SaveField(null, null)]
	public int draftMapIndex;

	[SaveField(null, null)]
	public EnvironmentInfluenceDef locationEnvironmentInfluence;

	[SaveField(null, null)]
	public InfluenceDef disorderCriticalInfluence;

	[SaveField(null, null)]
	public ForerunnerLegacy forerunnerLegacy;

	[SaveField(null, null)]
	public string overrideName;

	[SaveField(null, null)]
	public string overrideDescription;

	[SaveField(null, null)]
	public bool isCustomSpeicalLocation;

	public bool selectable
	{
		get
		{
			if (!jumpAble || visited)
			{
				return CSCommon.cheatJumpMap;
			}
			return true;
		}
	}

	public bool jumpAble => gameState.dungeonMapManager.CanJump(this);

	public bool reachable => gameState.dungeonMapManager.CanReach(this);

	public bool IsFightLocation => LocationGenerator.IsFightLocation(type);

	private PathLocation()
	{
	}

	public PathLocation(GameStateInfo gameState)
	{
		this.gameState = gameState;
		connections = new List<PathLocation>();
		ResetLocation();
	}

	public bool CanReach(PathLocation location)
	{
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.NoPathForkLimit) && location.localProgress == localProgress + 1)
		{
			return true;
		}
		if (gameState.dungeonMapManager.CanJumpByProgressDistance(this, location))
		{
			return true;
		}
		if (connections.Contains(location))
		{
			return true;
		}
		foreach (PathLocation connection in connections)
		{
			if (connection.CanReach(location))
			{
				return true;
			}
		}
		return false;
	}

	public string GetLocationDisplayName()
	{
		if (hideLoc)
		{
			return ScriptableObjectSingleton<DungeonGenerationManager>.instance.hideLocationDisplayConfig.name;
		}
		if (variantType != LocationVariantType.None)
		{
			return GetLocationVaraintTypeName(variantType);
		}
		return GetLocationTypeName(type);
	}

	public DraftPivot GetAttachDraftPivot(DraftMap draftMap)
	{
		return draftMap.GetIndexPivot(draftMapIndex);
	}

	public bool ShouldShow(DraftMap draftMap, ApplicationStateInfo applicationState)
	{
		return GetAttachDraftPivot(draftMap).ShouldShow(applicationState);
	}

	public static string GetLocationTypeName(LocationType type)
	{
		return ScriptableObjectSingleton<DungeonGenerationManager>.instance.locationDisplayConfigs[type].name;
	}

	public static string GetLocationVaraintTypeName(LocationVariantType type)
	{
		return ScriptableObjectSingleton<DungeonGenerationManager>.instance.locationVariantDisplayConfigs[type].name;
	}

	public Sprite GetLocationIcon()
	{
		if (hideLoc)
		{
			return ScriptableObjectSingleton<DungeonGenerationManager>.instance.hideLocationDisplayConfig.icon;
		}
		if (attachEncounterSetupInfo?.fixedEventTemplate is AwakeningHealSpringEvent)
		{
			return ScriptableObjectSingleton<DungeonGenerationManager>.instance.locationDisplayConfigs[LocationType.HealFire].icon;
		}
		if (variantType != LocationVariantType.None)
		{
			return ScriptableObjectSingleton<DungeonGenerationManager>.instance.locationVariantDisplayConfigs[variantType].icon;
		}
		return ScriptableObjectSingleton<DungeonGenerationManager>.instance.locationDisplayConfigs[type].icon;
	}

	public string GetLocationDescriptionText()
	{
		if (overrideDescription != null)
		{
			return overrideDescription;
		}
		if (hideLoc)
		{
			return ScriptableObjectSingleton<DungeonGenerationManager>.instance.hideLocationDisplayConfig.description;
		}
		if (variantType != LocationVariantType.None)
		{
			return ScriptableObjectSingleton<DungeonGenerationManager>.instance.locationVariantDisplayConfigs[variantType].description;
		}
		return ScriptableObjectSingleton<DungeonGenerationManager>.instance.locationDisplayConfigs[type].description;
	}

	private void ResetLocation()
	{
		finished = false;
		visited = false;
		attachEncounterSetupInfo = null;
		connections.RemoveAll((PathLocation c) => c == null);
		foreach (PathLocation connection in connections)
		{
			if (!connection.connections.Contains(this))
			{
				connection.connections.Add(this);
			}
		}
	}

	public static LocationType GetLocationVariantSourceLocation(LocationVariantType variantType)
	{
		return variantType switch
		{
			LocationVariantType.None => LocationType.None, 
			LocationVariantType.CrystalUpgradeFreeBless => LocationType.CrystalUpgradeRoom, 
			LocationVariantType.EliteFightHighLevelEnemy => LocationType.EliteFight, 
			LocationVariantType.UpgradeRoomFreeInitUpgrade => LocationType.UpgradeRoom, 
			LocationVariantType.GoldStoreMoreItem => LocationType.GoldStore, 
			LocationVariantType.AttachRuneDoubleRune => LocationType.AttachRune, 
			LocationVariantType.HealFireFullHeal => LocationType.HealFire, 
			LocationVariantType.GoldStoreEndlessRefresh => LocationType.GoldStore, 
			LocationVariantType.CrystalUpgradeEndlessRefreshAndGet => LocationType.CrystalUpgradeRoom, 
			LocationVariantType.AttachRuneEndlessRegenerate => LocationType.AttachRune, 
			LocationVariantType.FinalBoss => LocationType.Boss, 
			LocationVariantType.FinalEndingHealFire => LocationType.HealFire, 
			LocationVariantType.FadedHealFire => LocationType.HealFire, 
			LocationVariantType.HealthCostUpgradeRoom => LocationType.UpgradeRoom, 
			LocationVariantType.EnhanceRune => LocationType.AttachRune, 
			LocationVariantType.BigBonusLocation => LocationType.BonusGame, 
			LocationVariantType.AmbushFight => LocationType.NormalFight, 
			LocationVariantType.EliteVariant => LocationType.EliteFight, 
			_ => throw new ArgumentOutOfRangeException("variantType", variantType, null), 
		};
	}

	public static bool CheckLocationVariantRandomGenerateAbleToLoc(LocationVariantType variant, PathLocation loc, GameStateInfo gameStateInfo)
	{
		if (loc.type == LocationType.None)
		{
			return false;
		}
		if (GetLocationVariantSourceLocation(variant) != loc.type)
		{
			return false;
		}
		if (variant == LocationVariantType.EnhanceRune && !gameStateInfo.playerState.DeckContainer.GetAllItems().SelectMany((Item it) => it.Runes).Any((Rune r) => r.def.stacking))
		{
			return false;
		}
		return true;
	}
}
