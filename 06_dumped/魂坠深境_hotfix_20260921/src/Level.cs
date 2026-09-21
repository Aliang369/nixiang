using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using UnityEngine;

public class Level : CTScriptableObject
{
	public class EnvironmentInfluenceSpawnSetup
	{
		public EnvironmentInfluenceDef def;

		public float weight;
	}

	[SerializeField]
	[CTSOAssetName]
	[LocalizeText("场景名字")]
	public string Name;

	public bool initLock;

	public int stage;

	[SerializeField]
	public Sprite levelLoadingImage;

	[SerializeField]
	public List<DraftMapRef> draftMapRefs;

	[SerializeField]
	public List<RoomRef> extraRooms;

	[SerializeField]
	public List<EnemyAssetRef> enemyPool;

	[SerializeField]
	public List<WorldEvent> worldEventPool;

	[SerializeField]
	public SkillDef forerunnerFallbackSkill;

	[TextArea]
	[LocalizeText("场景先行者保底留言")]
	[SerializeField]
	public string forerunnerFallbackNote;

	public float environmentInfluenceBaseSpawnChance;

	public List<EnvironmentInfluenceSpawnSetup> environmentInfluenceSpawnSetups;

	[SerializeField]
	public EventReference normalBattleMusicRef;

	[SerializeField]
	public EventReference bossBattleMusicRef;

	[SerializeField]
	public EventReference peaceMusicRef;

	[SerializeField]
	public bool overrideDungeonMapColor;

	[SerializeField]
	public Gradient dungeonMapBackgroundColor;

	[SerializeField]
	public Gradient dungeonMapProgressColor;

	[SerializeField]
	public EventReference environmentMusicRef;

	public override string CTAssetIDPrepend => "LV";

	public void Unload(AssetCatalog assetCatalog)
	{
		foreach (EnemyInfoData leveLLoadEnemy in GetLeveLLoadEnemies(assetCatalog))
		{
			leveLLoadEnemy.Unload();
		}
		foreach (RoomRef item in GetAllValidLevelRoomRef(assetCatalog))
		{
			item.Unload(assetCatalog);
		}
	}

	public List<DraftMapRef> GetGeneratableDraftMaps(GameStateInfo gs)
	{
		return (from m in draftMapRefs.OrEmptyIfNull()
			where m?.IsValidRef(gs.applicationState.assetCatalog) ?? false
			select m).Where(delegate(DraftMapRef m)
		{
			DraftMapInfoData assetPreData = m.GetAssetPreData(gs.applicationState.assetCatalog);
			bool num = !assetPreData.isExtraMap || gs.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.EnableExtraMap);
			bool flag = assetPreData.minimumDifficultyLevel <= DifficultyLevel.Not || gs.difficultyState.contractDifficulty >= assetPreData.minimumDifficultyLevel;
			return num & flag;
		}).ToList();
	}

	public List<EnemyInfoData> GetLeveLLoadEnemies(AssetCatalog catalog)
	{
		List<EnemyInfoData> list = new List<EnemyInfoData>();
		list.AddRange(GetValidLevelEnemies(catalog));
		foreach (EnemyInfoData validAsset in catalog.enemyInfoDataCatalog.ValidAssets)
		{
			if (validAsset.globalSpawn)
			{
				list.Add(validAsset);
			}
		}
		return list;
	}

	public List<EnemyInfoData> GetValidLevelEnemies(AssetCatalog catalog)
	{
		if (enemyPool == null)
		{
			return new List<EnemyInfoData>(catalog.enemyInfoDataCatalog.ValidAssets);
		}
		return (from e in enemyPool
			where e?.IsValidRef(catalog) ?? false
			select e.GetAssetPreData(catalog)).ToList();
	}

	public List<RoomRef> GetAllValidLevelRoomRef(AssetCatalog catalog)
	{
		List<RoomRef> res = new List<RoomRef>();
		foreach (DraftMapRef item in draftMapRefs.OrEmptyIfNull())
		{
			if (item == null)
			{
				continue;
			}
			DraftMapInfoData assetPreData = item.GetAssetPreData(catalog);
			if (assetPreData == null)
			{
				continue;
			}
			foreach (RoomRef item2 in assetPreData.allRoomRefs.OrEmptyIfNull())
			{
				AddIfValid(item2);
			}
		}
		foreach (RoomRef extraRoom in extraRooms)
		{
			AddIfValid(extraRoom);
		}
		return res;
		void AddIfValid(RoomRef roomRef)
		{
			if (roomRef != null && roomRef.IsValidRef(catalog))
			{
				res.Add(roomRef);
			}
		}
	}

	public static List<Level> GetStageLevels(int stage, AssetCatalog catalog)
	{
		return catalog.levelDefCatalog.ValidAssets.FindAll((Level l) => l.stage == stage);
	}
}
