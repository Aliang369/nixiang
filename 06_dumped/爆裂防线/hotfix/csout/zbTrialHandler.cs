using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;

public class zbTrialHandler
{
	public ESkillType skillType;

	public int levelId;

	private long lastServerTm;

	private bool hasRequested;

	private Dictionary<ESkillType, EBattleFuncType> skillToTrialMap;

	private Dictionary<EBattleFuncType, ESkillType> trialToSkillMap;

	public void Init()
	{
		skillToTrialMap = new Dictionary<ESkillType, EBattleFuncType>
		{
			{
				ESkillType.Qiang,
				EBattleFuncType.HeroTrialQiang
			},
			{
				ESkillType.Pao,
				EBattleFuncType.HeroTrialPao
			},
			{
				ESkillType.Jiguang,
				EBattleFuncType.HeroTrialJiguang
			},
			{
				ESkillType.Bingdan,
				EBattleFuncType.HeroTrialBingdan
			},
			{
				ESkillType.Ranshaodan,
				EBattleFuncType.HeroTrialRanshaodan
			},
			{
				ESkillType.Hongzha,
				EBattleFuncType.HeroTrialHongzha
			},
			{
				ESkillType.Dianci,
				EBattleFuncType.HeroTrialDianci
			},
			{
				ESkillType.Wurenji,
				EBattleFuncType.HeroTrialWurenji
			},
			{
				ESkillType.Motuo,
				EBattleFuncType.HeroTrialMotuo
			},
			{
				ESkillType.Bingbao,
				EBattleFuncType.HeroTrialBingbao
			},
			{
				ESkillType.Shexian,
				EBattleFuncType.HeroTrialShexian
			},
			{
				ESkillType.Dianqiu,
				EBattleFuncType.HeroTrialDianqiu
			},
			{
				ESkillType.Longjuan,
				EBattleFuncType.HeroTrialLongjuan
			},
			{
				ESkillType.Fengren,
				EBattleFuncType.HeroTrialFengren
			}
		};
		trialToSkillMap = skillToTrialMap.ToDictionary((KeyValuePair<ESkillType, EBattleFuncType> kv) => kv.Value, (KeyValuePair<ESkillType, EBattleFuncType> kv) => kv.Key);
	}

	public void UpdatTrailInfo(HeroTrialInfo heroTrialInfo)
	{
		ResetRequstTime();
		zbServerData.sGameData.HeroTrialInfo = heroTrialInfo;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_HeroTrial_Data);
	}

	public void UpdateSkillType(ESkillType _skillType)
	{
		skillType = _skillType;
	}

	public void UpdateLevelId(int _levelId)
	{
		levelId = _levelId;
	}

	public EBattleFuncType GetBattleFuncType(ESkillType skillType)
	{
		if (!skillToTrialMap.TryGetValue(skillType, out var value))
		{
			return EBattleFuncType.EmptyValue;
		}
		return value;
	}

	public ESkillType GetSkillType(EBattleFuncType battleFuncType)
	{
		if (!trialToSkillMap.TryGetValue(battleFuncType, out var value))
		{
			return ESkillType.EmptyValue;
		}
		return value;
	}

	public long GetTrialShowEndTmByBattleFuncType(EBattleFuncType funcType)
	{
		if (zbServerData.sGameData.HeroTrialInfo.CurrentType != GetSkillType(funcType))
		{
			return 0L;
		}
		return zbServerData.sGameData.HeroTrialInfo.ShowEndTm;
	}

	public void Update()
	{
		CheckTrialDataReq();
	}

	public bool HasRedPoint()
	{
		if (zbServerData.sGameData.HeroTrialInfo != null && zbServerData.sGameData.HeroTrialInfo.DailyCount > 0 && zbSelf.Instance.timeHandler.GetServerTime() < zbServerData.sGameData.HeroTrialInfo.ShowEndTm)
		{
			return true;
		}
		return false;
	}

	public void CheckTrialDataReq()
	{
		if (!zbSelf.Instance.IsLoading() && zbIntervalTimer.Instance.IsFiveSecondPassed() && zbServerData.sGameData.HeroTrialInfo != null)
		{
			long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
			long num = zbServerData.sGameData.HeroTrialInfo.RefreshTm + 2;
			if (!hasRequested && lastServerTm < num && serverTime > num)
			{
				zbWsMsgHandler.Instance.HeroTrial_Data_Req();
				hasRequested = true;
			}
			lastServerTm = serverTime;
		}
	}

	public void ResetRequstTime()
	{
		lastServerTm = 0L;
		hasRequested = false;
	}
}
