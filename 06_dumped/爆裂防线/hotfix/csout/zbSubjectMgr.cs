using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using zbType;

public class zbSubjectMgr
{
	private static zbSubjectMgr sInstance;

	public static zbSubjectMgr Instance
	{
		get
		{
			if (sInstance == null)
			{
				sInstance = new zbSubjectMgr();
			}
			return sInstance;
		}
	}

	public void ShowLoginSubject()
	{
		if (!ChargebackInfoCheck() && !NeedCombatResume())
		{
			OnCallBackWallIdle(isOK: true);
		}
	}

	private void OnCallBackWallIdle(bool isOK)
	{
		JObject record = zbDataConfig.Instance.idleBaseJsonTable.GetRecord();
		JToken val = ((record != null) ? record["gain"] : null);
		if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Idle) && zbSelf.Instance.timeHandler.GetServerTime() - zbServerData.sGameData.IdleInfo?.GainTm > Extensions.Value<int>((IEnumerable<JToken>)val[(object)"settleInterval"]))
		{
			zbUIMgr.Instance.ShowWallIdle(OnCallBackSubject);
		}
		else
		{
			OnCallBackSubject(isOK: true);
		}
	}

	private void OnCallBackSubject(bool isOK)
	{
		if ((zbSelf.Instance.journeyOneHandler.IsActive() || zbSelf.Instance.journeyTwoHandler.IsActive() || zbSelf.Instance.journeyThreeHandler.IsActive()) && zbGameSave.Instance.GetString("SubjectJourneyDate" + zbSelf.Instance.GetRoleId()) != zbSelf.Instance.timeHandler.GetServerDateStr())
		{
			zbUIMgr.Instance.ShowSubjectJourney(OnCallBackHoliday);
		}
		else if (zbServerData.sGameData.MainQuestInfo.MainLevel >= 4 && (zbSelf.Instance.restaurantHandler.GetRestaurantDetail() != null || zbSelf.Instance.endlessMineHandler.GetEndlessMineDetail() != null || zbSelf.Instance.snowVeilHandler.GetSnowVeilDetail() != null) && zbGameSave.Instance.GetString("SubjectHolidayDate" + zbSelf.Instance.GetRoleId()) != zbSelf.Instance.timeHandler.GetServerDateStr())
		{
			zbUIMgr.Instance.ShowSubjectHoliday(OnCallBackHoliday);
		}
		else if (zbServerData.sGameData.MainQuestInfo.MainLevel >= 4 && zbSelf.Instance.bossSubjectHandler.GetBossSubjectDetail() != null && zbGameSave.Instance.GetString("SubjectBossDate" + zbSelf.Instance.GetRoleId()) != zbSelf.Instance.timeHandler.GetServerDateStr())
		{
			zbUIMgr.Instance.ShowSubjectBoss(OnCallBackHoliday);
		}
		else if (zbServerData.sGameData.MainQuestInfo.MainLevel >= 4 && zbSelf.Instance.wallSubjectHandler.GetWallSubjectDetail() != null && zbGameSave.Instance.GetString("SubjectWallDate" + zbSelf.Instance.GetRoleId()) != zbSelf.Instance.timeHandler.GetServerDateStr())
		{
			zbUIMgr.Instance.ShowSubjectWall(OnCallBackHoliday);
		}
		else if (zbServerData.sGameData.MainQuestInfo.MainLevel >= 4 && zbSelf.Instance.mechaSubjectHandler.GetMechaSubjectDetail() != null && zbGameSave.Instance.GetString("SubjectMechaDate" + zbSelf.Instance.GetRoleId()) != zbSelf.Instance.timeHandler.GetServerDateStr())
		{
			zbUIMgr.Instance.ShowSubjectMecha(OnCallBackHoliday);
		}
		else
		{
			OnCallBackHoliday(isOK: true);
		}
	}

	private void OnCallBackHoliday(bool isOK)
	{
		if (zbServerData.sGameData.MainQuestInfo.MainLevel < 5)
		{
			OnCallBackRecharge(isOK: true);
			return;
		}
		if (zbServerData.sGameData.OperateInfo?.InfiniteGiftList != null)
		{
			foreach (InfiniteGiftDetail infiniteGift in zbServerData.sGameData.OperateInfo.InfiniteGiftList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(infiniteGift.BaseInfo) && zbSelf.Instance.operateHandler.HasInfiniteGift(infiniteGift.BaseInfo.UUID))
				{
					zbUIMgr.Instance.ShowInfiniteGift(infiniteGift, OnCallBackRecharge);
					return;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo?.LimitTimeRouletteList != null && zbGameSave.Instance.GetString("SubjectDate" + zbSelf.Instance.GetRoleId()) != zbSelf.Instance.timeHandler.GetServerDateStr())
		{
			foreach (LimitTimeRouletteDetail value in zbServerData.sGameData.OperateInfo.LimitTimeRouletteList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(value.BaseInfo))
				{
					continue;
				}
				zbUIMgr.Instance.ShowSubject(ESubjectType.Roulette, OnCallBackRecharge, delegate(bool flag)
				{
					if (flag)
					{
						zbUIMgr.Instance.ShowLimitTimeRoulette(value);
					}
				});
				return;
			}
		}
		if (zbServerData.sGameData.OperateInfo?.LimitTimeTreasureList != null && zbGameSave.Instance.GetString("SubjectDate" + zbSelf.Instance.GetRoleId()) != zbSelf.Instance.timeHandler.GetServerDateStr())
		{
			foreach (LimitTimeRouletteDetail value2 in zbServerData.sGameData.OperateInfo.LimitTimeTreasureList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(value2.BaseInfo))
				{
					continue;
				}
				zbUIMgr.Instance.ShowSubject(ESubjectType.Treasure, OnCallBackRecharge, delegate(bool flag)
				{
					if (flag)
					{
						zbUIMgr.Instance.ShowLimitTimeTreasure(value2);
					}
				});
				return;
			}
		}
		if (zbServerData.sGameData.OperateInfo?.LimitTimeVendorList != null && zbGameSave.Instance.GetString("SubjectDate" + zbSelf.Instance.GetRoleId()) != zbSelf.Instance.timeHandler.GetServerDateStr())
		{
			foreach (LimitTimeRouletteDetail value3 in zbServerData.sGameData.OperateInfo.LimitTimeVendorList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(value3.BaseInfo))
				{
					continue;
				}
				zbUIMgr.Instance.ShowSubject(ESubjectType.Vendor, OnCallBackRecharge, delegate(bool flag)
				{
					if (flag)
					{
						zbUIMgr.Instance.ShowLimitTimeVendor(value3);
					}
				});
				return;
			}
		}
		if (zbServerData.sGameData.OperateInfo?.LimitTimeCatcherList != null && zbGameSave.Instance.GetString("SubjectOperateDate" + zbSelf.Instance.GetRoleId()) != zbSelf.Instance.timeHandler.GetServerDateStr())
		{
			foreach (LimitTimeCatcherDetail limitTimeCatcher in zbServerData.sGameData.OperateInfo.LimitTimeCatcherList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(limitTimeCatcher.BaseInfo))
				{
					zbUIMgr.Instance.ShowSubjectOperate(EOperateType.LimitTimeCatcher, OnCallBackRecharge);
					return;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo?.ZhanLingKillList != null && zbGameSave.Instance.GetString("SubjectOperateDate" + zbSelf.Instance.GetRoleId()) != zbSelf.Instance.timeHandler.GetServerDateStr())
		{
			foreach (ZhanLingKillDetail zhanLingKill in zbServerData.sGameData.OperateInfo.ZhanLingKillList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(zhanLingKill.BaseInfo))
				{
					zbUIMgr.Instance.ShowSubjectOperate(EOperateType.ZhanLingKill, OnCallBackRecharge);
					return;
				}
			}
		}
		OnCallBackRecharge(isOK: true);
	}

	private void OnCallBackRecharge(bool isOK)
	{
		if (zbSelf.Instance.activityHandler.CanFirstRechargeGet() || (zbSelf.Instance.activityHandler.IsFirstRechargeUnActive() && zbGameSave.Instance.GetString("SubjectFirstRechargeDate" + zbSelf.Instance.GetRoleId()) != zbSelf.Instance.timeHandler.GetServerDateStr()))
		{
			zbUIMgr.Instance.ShowFirstRecharge(OnCallBackLegionWar);
			zbGameSave.Instance.SetString("SubjectFirstRechargeDate" + zbSelf.Instance.GetRoleId(), zbSelf.Instance.timeHandler.GetServerDateStr());
			return;
		}
		bool flag = zbServerData.sGameData.MainQuestInfo.MainLevel >= 5 && !zbSelf.Instance.vipHandler.ValidateHasWeekCard() && zbGameSave.Instance.GetString($"SubjectDate{zbSelf.Instance.GetRoleId()}") != zbSelf.Instance.timeHandler.GetServerDateStr();
		bool flag2 = zbServerData.sGameData.MainQuestInfo.MainLevel >= 5 && zbServerData.sGameData.RechargeActiveInfo.PassportInfo.BaseInfo.Active == EBoolState.False && zbGameSave.Instance.GetString($"SubjectDate{zbSelf.Instance.GetRoleId()}") != zbSelf.Instance.timeHandler.GetServerDateStr();
		if (flag2 & flag)
		{
			if (Random.value < 0.5f)
			{
				zbUIMgr.Instance.ShowSubjectCard(OnCallBackLegionWar);
				return;
			}
			zbUIMgr.Instance.ShowSubject(ESubjectType.Passport, OnCallBackLegionWar, delegate(bool ok)
			{
				if (ok)
				{
					zbUIMgr.Instance.ShowVip(4, 1);
				}
			});
		}
		else if (flag)
		{
			zbUIMgr.Instance.ShowSubjectCard(OnCallBackLegionWar);
		}
		else if (flag2)
		{
			zbUIMgr.Instance.ShowSubject(ESubjectType.Passport, OnCallBackLegionWar, delegate(bool flag3)
			{
				if (flag3)
				{
					zbUIMgr.Instance.ShowVip(4, 1);
				}
			});
		}
		else
		{
			OnCallBackLegionWar(isOK: true);
		}
	}

	private void OnCallBackLegionWar(bool isOK)
	{
		if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.LegionWar))
		{
			zbSelf.Instance.timeHandler.IsSubjectLegionWarTime();
		}
	}

	private void OnMsgPvpConfirm(bool isOK)
	{
		if (isOK)
		{
			zbUIMgr.Instance.ShowPvpModSelect();
		}
		else
		{
			zbWsMsgHandler.Instance.PvpSet_ModCancel_Req();
		}
	}

	private bool NeedCombatResume()
	{
		PvpInfo pvpInfo = zbServerData.sGameData.PvpInfo;
		if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Arena) && pvpInfo != null && (pvpInfo.SetModInfo.SkillList.Count > 0 || pvpInfo.ValidModInfo.SelectList.Count <= 0))
		{
			if (pvpInfo.SetModInfo.SkillList.Count > 0)
			{
				if (pvpInfo.ValidModInfo.SelectList.Count <= 0)
				{
					zbUIMgr.Instance.ShowMessageConfirm(zbUtil.Language("lang.11476"), delegate
					{
						zbUIMgr.Instance.ShowPvpModSelect();
					});
					return true;
				}
				zbUIMgr.Instance.ShowMessageBox("Tip", zbUtil.Language("lang.11471"), OnMsgPvpConfirm, zbUtil.Language("lang.11472"), zbUtil.Language("lang.10873"));
				return true;
			}
			return true;
		}
		if (zbServerData.sGameData.WorldInfo.DungeonBaseInfo != null && (zbServerData.sGameData.WorldInfo.DungeonBaseInfo.PassState == EPassState.InProgress || zbServerData.sGameData.WorldInfo.DungeonBaseInfo.PassState == EPassState.Passed))
		{
			zbUIMgr.Instance.ShowDungeonResume(isWorldDungeon: true);
			return true;
		}
		zbCombatRecorder zbCombatRecorder2 = new zbCombatRecorder();
		zbCombatRecorder2.Init();
		if (zbCombatRecorder2.Load())
		{
			if (zbCombatRecorder2.loadedData.recordCombatSystem.battleInfo.BattleBaseInfo.BattleType == EBattleFuncType.LegionWar)
			{
				zbCombatRecorder2.ClearRecord();
				zbWsMsgHandler.Instance.LegionWar_FightCancel_Req();
				return false;
			}
			if (zbCombatRecorder2.loadedData.recordCombatSystem.battleInfo.BattleBaseInfo.BattleType == EBattleFuncType.Facility)
			{
				zbCombatRecorder2.ClearRecord();
				return false;
			}
			zbUIMgr.Instance.ShowCombatResume(zbCombatRecorder2);
			return true;
		}
		return false;
	}

	private bool ChargebackInfoCheck()
	{
		if (zbServerData.sGameData.RechargeActiveInfo != null && zbServerData.sGameData.RechargeActiveInfo.ChargebackInfo != null)
		{
			ChargebackInfo chargebackInfo = zbServerData.sGameData.RechargeActiveInfo.ChargebackInfo;
			if (chargebackInfo.State == EChargebackState.Warning || chargebackInfo.State == EChargebackState.Danger)
			{
				zbUIMgr.Instance.ShowVipRefund();
				return true;
			}
		}
		return false;
	}
}
