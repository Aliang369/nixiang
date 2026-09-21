using System;
using System.Collections.Generic;
using System.Linq;

public class PVPModeManager : GlobalGameManager
{
	public class DeckPresetCompStorage
	{
		[SaveField(null, null)]
		public DeckPresetCompDef def;

		[SaveField(null, null)]
		public int count;

		private DeckPresetCompStorage()
		{
		}

		public DeckPresetCompStorage(DeckPresetCompDef def, int count)
		{
			this.def = def;
			this.count = count;
		}
	}

	[SaveField(null, null)]
	public List<EnemyInfoData> unlockedPVPEnemyAvatars;

	[SaveField(null, null)]
	public List<DeckPresetCompStorage> storagePVPDeckPresetComps;

	[SaveField(null, null)]
	public PVPDefender currentDefender;

	[SaveField(null, null)]
	public PVPDefender testBattleDefender;

	[SaveField(null, null)]
	public bool testBattleDefenderEditMode;

	[SaveField(null, null)]
	public PVPChanllengeGameRecord challengeRecordThisGame;

	[NonSerialized]
	public bool pvpModeProfileDataLoaded;

	[NonSerialized]
	public bool pvpModeProfileDataLoading;

	[NonSerialized]
	public GameAsyncOperation pvpModeProfileDataLoadingOperation;

	public int ChallengeCountToday => base.applicationState.recordManager.records.Where((GlobalGameRecordManager.GameRecordPreviewInfo r) => ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentDailyPeriod == ScriptableObjectSingleton<CalendarDataManager>.instance.GetDailyPeriod(r.finishDateTime)).Count((GlobalGameRecordManager.GameRecordPreviewInfo r) => r.contract.contractDef == ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpOffenderContractDef);

	public IEnumerable<GlobalGameRecordManager.GameRecordPreviewInfo> AllChallengeRecordHistory => base.applicationState.recordManager.records.Where((GlobalGameRecordManager.GameRecordPreviewInfo r) => r.pvpChanllengeRecord != null);

	public override void Init(ApplicationStateInfo applicationState)
	{
		if (unlockedPVPEnemyAvatars == null)
		{
			unlockedPVPEnemyAvatars = new List<EnemyInfoData>();
		}
		if (storagePVPDeckPresetComps == null)
		{
			storagePVPDeckPresetComps = new List<DeckPresetCompStorage>();
		}
		pvpModeProfileDataLoaded = !GlobalConfigManager.instance.enableNetwork;
		pvpModeProfileDataLoading = false;
		pvpModeProfileDataLoadingOperation = null;
		applicationState.eventManager.gameEvent.AddGlobalListener(_OnGameEvent);
	}

	public override void Dispose()
	{
		base.applicationState.eventManager.gameEvent.RemoveGlobalListener(_OnGameEvent);
	}

	public void UnlockEnemyAvatar(EnemyInfoData enemyAvatar)
	{
		if (!unlockedPVPEnemyAvatars.Any((EnemyInfoData s) => s.FullId == enemyAvatar.FullId))
		{
			unlockedPVPEnemyAvatars.Add(enemyAvatar);
		}
	}

	public void AddPVPDeckPresetComp(DeckPresetCompDef comp)
	{
		int num = storagePVPDeckPresetComps.FindIndex((DeckPresetCompStorage c) => c.def.UniqueID == comp.UniqueID);
		if (num >= 0)
		{
			storagePVPDeckPresetComps[num].count++;
		}
		else
		{
			storagePVPDeckPresetComps.Add(new DeckPresetCompStorage(comp, 1));
		}
	}

	public void TestUnlockAll()
	{
		foreach (EnemyAssetRef item in GetAllCatalogPVPEnemyAvatar())
		{
			unlockedPVPEnemyAvatars.Add(item.GetAssetPreData(base.applicationState.assetCatalog));
		}
	}

	public List<DeckPresetCompDef> GetAllCatalogPVPComp()
	{
		List<DeckPresetCompDef> list = new List<DeckPresetCompDef>();
		foreach (ItemDefinition validAsset in base.applicationState.assetCatalog.itemDefCatalog.ValidAssets)
		{
			if (validAsset.IsPVPComp)
			{
				list.Add(new DeckPresetCompDef(validAsset));
			}
		}
		foreach (InfluenceDef validAsset2 in base.applicationState.assetCatalog.influenceCatalog.ValidAssets)
		{
			if (validAsset2.IsPVPComp)
			{
				list.Add(new DeckPresetCompDef(validAsset2));
			}
		}
		foreach (RuneDef validAsset3 in base.applicationState.assetCatalog.runeDefCatalog.ValidAssets)
		{
			if (validAsset3.IsPVPComp)
			{
				list.Add(new DeckPresetCompDef(validAsset3));
			}
		}
		return list;
	}

	public List<EnemyAssetRef> GetAllCatalogPVPEnemyAvatar()
	{
		List<EnemyAssetRef> list = new List<EnemyAssetRef>();
		foreach (EnemyInfoData validAsset in base.applicationState.assetCatalog.enemyInfoDataCatalog.ValidAssets)
		{
			if (validAsset.isPvpEnemy)
			{
				list.Add(new EnemyAssetRef(validAsset));
			}
		}
		return list;
	}

	private void _OnGameEvent(GameEvent.Type type, GameEvent gameEvent)
	{
		if (type == GameEvent.Type.BeforeGameStart)
		{
			challengeRecordThisGame = null;
		}
		if (!(gameEvent.gameState.contractState.selectedContract.contractDef != ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpOffenderContractDef))
		{
			if (type == GameEvent.Type.BeforeGameStart)
			{
				challengeRecordThisGame = new PVPChanllengeGameRecord(gameEvent.gameState.GameGUID);
			}
			if (challengeRecordThisGame != null && type == GameEvent.Type.BeforeBattleEnd)
			{
				PVPDefenderBattleRecord item = new PVPDefenderBattleRecord(gameEvent.gameState.battleState, gameEvent.battleFinishType);
				challengeRecordThisGame.defenderBattleRecords.Add(item);
			}
		}
	}
}
