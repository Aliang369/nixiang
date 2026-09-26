using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using zbType;

public class zbElementTrialHandler
{
	public ESkillAttackType m_attackType;

	public int m_levelId;

	public void Init()
	{
	}

	public zbZhanlingElementCsvRecord GetGrandPrizeRecord()
	{
		int num = 0;
		foreach (zbZhanlingElementCsvRecord value in zbDataConfig.Instance.zhanlingElementCsvTable.GetRecords().records.Values)
		{
			if (num < value.trialLevel)
			{
				num = value.trialLevel;
			}
		}
		return zbDataConfig.Instance.zhanlingElementCsvTable.GetRecordByKey(num);
	}

	public int GetLevelTotal()
	{
		int num = 0;
		foreach (TrialDetail trial in zbServerData.sGameData.ElementTrialInfo.TrialList)
		{
			num += trial.HistoryLevel;
		}
		return num;
	}

	public Sprite GetBannerSpriteByAttackType(ESkillAttackType skillAttackType)
	{
		return (Sprite)(skillAttackType switch
		{
			ESkillAttackType.Phy => zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/trial/trialType_bg5.png"), 
			ESkillAttackType.Ice => zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/trial/trialType_bg0.png"), 
			ESkillAttackType.Fire => zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/trial/trialType_bg3.png"), 
			ESkillAttackType.Energy => zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/trial/trialType_bg4.png"), 
			ESkillAttackType.Elec => zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/trial/trialType_bg1.png"), 
			ESkillAttackType.Wind => zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/trial/trialType_bg2.png"), 
			_ => zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/trial/trialType_bg2.png"), 
		});
	}

	public TrialDetail GetTrialInfoByAttackType(ESkillAttackType skillAttackType)
	{
		TrialDetail result = null;
		foreach (TrialDetail trial in zbServerData.sGameData.ElementTrialInfo.TrialList)
		{
			if (trial.TrialType == (int)skillAttackType)
			{
				result = trial;
			}
		}
		return result;
	}

	public bool IsElementTrialOpened()
	{
		if (zbServerData.sGameData.ElementTrialInfo != null)
		{
			if (zbServerData.sGameData.ElementTrialInfo.CombatCount > 0 || zbServerData.sGameData.ElementTrialInfo.BuyCombat > 0)
			{
				return true;
			}
			foreach (TrialDetail trial in zbServerData.sGameData.ElementTrialInfo.TrialList)
			{
				if (trial.Level > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CanElementTrialCombat()
	{
		ElementTrialInfo elementTrialInfo = zbServerData.sGameData.ElementTrialInfo;
		return elementTrialInfo.DailyCount - elementTrialInfo.CombatCount > 0;
	}

	public bool CanElementTrialBuyCount()
	{
		int num = zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.WorldLairBuy, zbSelf.Instance.GetVipLevel());
		return zbServerData.sGameData.ElementTrialInfo.BuyCombat < num;
	}

	public void UpdateElementTrialInfo(ESkillAttackType type, int levelId)
	{
		m_attackType = type;
		m_levelId = levelId;
	}

	public int GetElementZhanlingTotalLevel()
	{
		int result = 0;
		if (zbServerData.sGameData.ElementTrialInfo != null)
		{
			result = zbServerData.sGameData.ElementTrialInfo.TotalLevel;
		}
		return result;
	}

	public bool CheckCanFight(int level)
	{
		_ = zbServerData.sGameData.ElementTrialInfo.TrialList;
		zbElementTrialLevelCsvRecord recordByKey = zbDataConfig.Instance.elementTrialLevelCsvTable.GetRecordByKey(level);
		if (level >= GetMaxLevel())
		{
			return false;
		}
		int unlockTotal = recordByKey.unlockTotal;
		return zbServerData.sGameData.ElementTrialInfo.TotalLevel >= unlockTotal;
	}

	public bool IsElementLairCombat(EBattleFuncType battleFuncType)
	{
		if (battleFuncType != EBattleFuncType.ElementTrialElec && battleFuncType != EBattleFuncType.ElementTrialEnergy && battleFuncType != EBattleFuncType.ElementTrialFire && battleFuncType != EBattleFuncType.ElementTrialIce && battleFuncType != EBattleFuncType.ElementTrialPhy)
		{
			return battleFuncType == EBattleFuncType.ElementTrialWind;
		}
		return true;
	}

	public int GetMaxLevel()
	{
		return zbDataConfig.Instance.elementTrialLevelCsvTable.GetRecords().records.Count;
	}

	public bool CheckElementTrialIsUnLock(ESkillAttackType type)
	{
		RepeatedField<HeroDetail> heroList = zbServerData.sGameData.HeroList;
		bool result = false;
		switch (type)
		{
		case ESkillAttackType.Ice:
			foreach (HeroDetail item in heroList)
			{
				ESkillType type7 = (ESkillType)zbDataConfig.Instance.heroCsvTable.GetRecordByKey(item.HeroTid).type;
				if (type7 == ESkillType.Bingbao || type7 == ESkillType.Bingdan)
				{
					result = true;
					break;
				}
			}
			break;
		case ESkillAttackType.Fire:
			foreach (HeroDetail item2 in heroList)
			{
				ESkillType type6 = (ESkillType)zbDataConfig.Instance.heroCsvTable.GetRecordByKey(item2.HeroTid).type;
				if (type6 == ESkillType.Pao || type6 == ESkillType.Ranshaodan)
				{
					result = true;
					break;
				}
			}
			break;
		case ESkillAttackType.Phy:
			foreach (HeroDetail item3 in heroList)
			{
				ESkillType type5 = (ESkillType)zbDataConfig.Instance.heroCsvTable.GetRecordByKey(item3.HeroTid).type;
				if (type5 == ESkillType.Motuo || type5 == ESkillType.Wurenji || type5 == ESkillType.Qiang || type5 == ESkillType.Hongzha)
				{
					result = true;
					break;
				}
			}
			break;
		case ESkillAttackType.Energy:
			foreach (HeroDetail item4 in heroList)
			{
				ESkillType type4 = (ESkillType)zbDataConfig.Instance.heroCsvTable.GetRecordByKey(item4.HeroTid).type;
				if (type4 == ESkillType.Jiguang || type4 == ESkillType.Shexian)
				{
					result = true;
					break;
				}
			}
			break;
		case ESkillAttackType.Elec:
			foreach (HeroDetail item5 in heroList)
			{
				ESkillType type3 = (ESkillType)zbDataConfig.Instance.heroCsvTable.GetRecordByKey(item5.HeroTid).type;
				if (type3 == ESkillType.Dianci || type3 == ESkillType.Dianqiu)
				{
					result = true;
					break;
				}
			}
			break;
		case ESkillAttackType.Wind:
			foreach (HeroDetail item6 in heroList)
			{
				ESkillType type2 = (ESkillType)zbDataConfig.Instance.heroCsvTable.GetRecordByKey(item6.HeroTid).type;
				if (type2 == ESkillType.Longjuan || type2 == ESkillType.Fengren)
				{
					result = true;
					break;
				}
			}
			break;
		}
		return result;
	}

	public bool HasRedPoint()
	{
		if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.ElementTrial))
		{
			ElementTrialInfo elementTrialInfo = zbServerData.sGameData.ElementTrialInfo;
			return elementTrialInfo.DailyCount - elementTrialInfo.CombatCount > 0;
		}
		return false;
	}
}
