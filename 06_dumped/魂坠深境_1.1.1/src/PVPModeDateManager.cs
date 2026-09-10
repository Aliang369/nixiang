using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PVPModeDateManager", menuName = "Singleton/PVPModeDateManager")]
public class PVPModeDateManager : ScriptableObjectSingleton<PVPModeDateManager>
{
	public UnityDateTime pvpModeUnlockTime;

	public PurchaseGroupDef pvpModePurchaseGroup;

	public int maxChallengeCountPerDay;

	public Dictionary<QualityLevel, int> pvpCompDrawWeightDic;

	public Dictionary<DeckPresetCompType, Sprite> compTypeIconDic;

	public DraftMapRef pvpChallengeMapRef;

	public int defenderNameMaxLength = 10;

	public int pvpDefenderPerGame;

	public MatterDef pvpBonusMatter;

	public int pvpMatterBonusOnChallengeGameWin;

	public int pvpMatterBonusPerChallengeBattleWin;

	public int pvpMatterBonusPerDefenderDamage;

	public int pvpMatterBonusPerDefenderBattleWin;

	public int defenderJoinRankMinimalBattleCount;

	public int resetPVPCompStorageSize;

	public Dictionary<DeckPresetCompType, int> typeCompMaxCountOnReset;

	public List<MatterInfo> resetPVPCompCostMatters;

	public List<EnemyAssetRef> initUnlockAvatars;

	public ContractDef pvpDefenderTestContractDef;

	public ContractDef pvpOffenderContractDef;

	public PVPEndPointWorldEvent pvpEndPointWorldEvent;

	public RoomRef pvpModeTestBattleRoomRef;

	public bool IsPVPModeGameUnlocked
	{
		get
		{
			if (!(DateTime.UtcNow >= pvpModeUnlockTime.UtcDateTime))
			{
				return Application.isEditor;
			}
			return true;
		}
	}

	public TimeSpan RemainTimeToUnlockPVPMode
	{
		get
		{
			if (!IsPVPModeGameUnlocked)
			{
				return pvpModeUnlockTime.UtcDateTime - DateTime.UtcNow;
			}
			return TimeSpan.Zero;
		}
	}

	public List<EnemyInfoData> GetAllPVPAvatar(AssetCatalog catalog)
	{
		return catalog.enemyInfoDataCatalog.ValidAssets.Where((EnemyInfoData e) => e.isPvpEnemy).ToList();
	}

	public List<DeckPresetCompDef> GetAllPVPComps(AssetCatalog catalog)
	{
		List<DeckPresetCompDef> list = new List<DeckPresetCompDef>();
		list.AddRange(from it in catalog.itemDefCatalog.ValidAssets
			where it.IsPVPComp
			select new DeckPresetCompDef(it));
		list.AddRange(from inf in catalog.influenceCatalog.ValidAssets
			where inf.IsPVPComp
			select new DeckPresetCompDef(inf));
		list.AddRange(from r in catalog.runeDefCatalog.ValidAssets
			where r.IsPVPComp
			select new DeckPresetCompDef(r));
		return list;
	}
}
