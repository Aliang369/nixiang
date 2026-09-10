using System;
using System.Collections.Generic;
using Sirenix.Utilities;

public class GlobalUnlockManager : GlobalGameManager
{
	[SaveField(null, null)]
	public List<ICTAsset> gotObject;

	[SaveField(null, null)]
	public HashSet<ICTAsset> unlockedResource;

	[SaveField(null, null)]
	public LevelLine globalLevelLine;

	public HashSet<ICTAsset> lockedResourceCache;

	[SaveField(null, null)]
	private HashSet<ICTAsset> newUnlockedResource;

	public override void Init(ApplicationStateInfo applicationState)
	{
		if (gotObject == null)
		{
			gotObject = new List<ICTAsset>();
		}
		if (globalLevelLine == null)
		{
			globalLevelLine = new LevelLine(ScriptableObjectSingleton<FortressDataManager>.instance.globalLevelLine);
		}
		if (unlockedResource == null)
		{
			unlockedResource = new HashSet<ICTAsset>();
			newUnlockedResource = new HashSet<ICTAsset>();
		}
		InitLockEntries();
	}

	public void LockResource(ICTAsset asset)
	{
		unlockedResource.Remove(asset);
	}

	public void UnlockResource(ICTAsset asset)
	{
		unlockedResource.Add(asset);
		newUnlockedResource.Add(asset);
	}

	public bool IsUnlocked(ICTAsset asset)
	{
		if (lockedResourceCache.Contains(asset))
		{
			return unlockedResource.Contains(asset);
		}
		return true;
	}

	public bool CheckNewUnlockAndTryReset(ICTAsset asset)
	{
		if (newUnlockedResource.Contains(asset))
		{
			newUnlockedResource.Remove(asset);
			return true;
		}
		return false;
	}

	private void InitLockEntries()
	{
		lockedResourceCache = GetAllInitLockEntries(base.applicationState.assetCatalog);
	}

	public static HashSet<ICTAsset> GetAllInitLockEntries(AssetCatalog catalog)
	{
		HashSet<ICTAsset> hashSet = new HashSet<ICTAsset>();
		foreach (LevelLineDef validAsset in catalog.levelLineDefCatalog.ValidAssets)
		{
			foreach (LevelLineDef.LineLevel level in validAsset.levels)
			{
				hashSet.AddRange(GetLockedEntryFormReward(level.reward));
				hashSet.AddRange(GetLockedEntryFormReward(level.appendReward));
			}
		}
		foreach (IdolDef validAsset2 in catalog.idolDefCatalog.ValidAssets)
		{
			foreach (IdolLevel level2 in validAsset2.levels)
			{
				foreach (GlobalRewardSetup item in level2.reward)
				{
					hashSet.AddRange(GetLockedEntryFormReward(item));
				}
			}
		}
		foreach (ClavisDef validAsset3 in catalog.clavisDefDefCatalog.ValidAssets)
		{
			foreach (GlobalRewardSetup activateReward in validAsset3.activateRewards)
			{
				hashSet.AddRange(GetLockedEntryFormReward(activateReward));
			}
		}
		foreach (PlayerCharacterDef validAsset4 in catalog.characterDefCatalog.ValidAssets)
		{
			if (validAsset4.initLock)
			{
				hashSet.Add(validAsset4);
			}
		}
		foreach (Level validAsset5 in catalog.levelDefCatalog.ValidAssets)
		{
			if (validAsset5.initLock)
			{
				hashSet.Add(validAsset5);
			}
		}
		return hashSet;
		static List<ICTAsset> GetLockedEntryFormReward(GlobalRewardSetup setup)
		{
			if (setup == null)
			{
				return new List<ICTAsset>();
			}
			if (setup is UnlockCTSOGlobalReward unlockCTSOGlobalReward)
			{
				return unlockCTSOGlobalReward.unlocks.ConvertAll((Converter<CTScriptableObject, ICTAsset>)((CTScriptableObject ctso) => ctso));
			}
			return new List<ICTAsset>();
		}
	}

	public void UnlockAll()
	{
		foreach (PlayerCharacterState characterState in base.applicationState.characterManager.characterStates)
		{
			characterState.Unlock("全解锁");
			if (characterState.def.talentTree != null)
			{
				TalentSlotCell[] talentSlotCells = characterState.def.talentTree.TalentSlotCells;
				foreach (TalentSlotCell talentSlotCell in talentSlotCells)
				{
					characterState.UnlockTalent(talentSlotCell.talent, base.applicationState);
				}
			}
		}
		foreach (ICTAsset item in lockedResourceCache)
		{
			if (!IsUnlocked(item))
			{
				UnlockResource(item);
			}
		}
		foreach (EnemyInfoData validAsset in base.applicationState.assetCatalog.enemyInfoDataCatalog.ValidAssets)
		{
			base.applicationState.recordManager.cutsceneDebutFinishedEnemies.Add(validAsset);
		}
		UnlockAllItemSkinAndUpgrades();
		newUnlockedResource.Clear();
	}

	private void UnlockAllItemSkinAndUpgrades()
	{
		base.applicationState.guideManager.itemSkinRecommendedItems.Clear();
		foreach (ItemSkinDef validAsset in base.applicationState.assetCatalog.itemSkinCatalog.ValidAssets)
		{
			base.applicationState.itemSkinManager.AddItemSkin(validAsset, addToRecommendation: false);
		}
	}

	public void RecordGotObject(ICTAsset obj)
	{
		gotObject.AddUnique(obj);
	}
}
