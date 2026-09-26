using Proto.Enums;
using Proto.Types;
using UnityEngine;

public class zbCombatState_Balance(zbCombatStateMachine fsm, ECombatState eState) : zbCombatState(fsm, eState)
{
	private float againCaclResult;

	private int reconnectNum;

	private bool zuobi;

	private bool curFrameConnect = true;

	public override void OnEnter()
	{
		curFrameConnect = false;
		base.OnEnter();
		reconnectNum = 1;
		againCaclResult = 0f;
		zbAudioMgr.Instance.StopAllAudio();
		Time.timeScale = zbCombatSpeed.GetSpeedByEnum(ECombatSpeed.X1);
		zbCombatSystem.Instance.Pause();
		if (zbCombatSystem.Instance.dataResult != null || zbCombatSystem.Instance.GetBattleType() == EBattleFuncType.EmptyValue)
		{
			zbUIMgr.Instance.DestroyWnd(EWndId.Combat);
			if (zbSelf.Instance.arenaHandler.isCombatRecord)
			{
				zbSelf.Instance.arenaHandler.isCombatRecord = false;
				zbUIMgr.Instance.ShowArenaRecordWnd(zbSelf.Instance.arenaHandler.arenaRecords);
			}
			else
			{
				zbUIMgr.Instance.ShowMainLevel();
			}
		}
		else
		{
			zuobi = zbSelf.Instance.combatHandler.IsZuobi();
			if (zuobi)
			{
				OnZuobiConfirm();
			}
			else
			{
				Reconnect();
			}
		}
	}

	public void Reconnect()
	{
		curFrameConnect = true;
		BattleResultInfo battleResult = zbCombatSystem.Instance.combatResult.CalcFightResult();
		switch (zbCombatSystem.Instance.GetBattleType())
		{
		case EBattleFuncType.EmptyValue:
			zbUIMgr.Instance.DestroyWnd(EWndId.Combat);
			zbUIMgr.Instance.ShowMainLevel();
			break;
		case EBattleFuncType.DungeonEasyFight:
		case EBattleFuncType.DungeonRiotFight:
		case EBattleFuncType.DungeonBossFight:
			if (zbSelf.Instance.worldDungeonHandler.IsWorldDungeon())
			{
				zbWsMsgHandler.Instance.WorldDungeon_EventFightResult_Req(battleResult);
			}
			break;
		case EBattleFuncType.MainQuest:
			zbWsMsgHandler.Instance.MainQuest_FightResult_Req(zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId, battleResult);
			break;
		case EBattleFuncType.EliteQuest:
			zbWsMsgHandler.Instance.EliteQuest_FightResult_Req(zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId, battleResult, zbSelf.Instance.eliteQuestHandler.fightCount, zbSelf.Instance.eliteQuestHandler.playerId);
			break;
		case EBattleFuncType.LegionBoss:
			zbWsMsgHandler.Instance.LegionBoss_FightResult_Req(zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId, battleResult);
			break;
		case EBattleFuncType.WorldBoss:
			zbWsMsgHandler.Instance.WorldBoss_FightResult_Req(zbSelf.Instance.worldBossHandler.worldBossuuid, battleResult, zbCombatSystem.Instance.combatData.masterTeam.combatStat.damageBossSum);
			break;
		case EBattleFuncType.WorldEventFightDay:
		case EBattleFuncType.WorldEventFightNight:
			zbWsMsgHandler.Instance.WorldEvent_FightResult_Req(zbCombatSystem.Instance.paramId, battleResult);
			break;
		case EBattleFuncType.WorldLair:
			zbWsMsgHandler.Instance.WorldLair_FightResult_Req(battleResult);
			break;
		case EBattleFuncType.WorldLairHelp:
			zbWsMsgHandler.Instance.WorldLair_HelpFightResult_Req(zbSelf.Instance.worldLairHandler.worldLairUuid, battleResult);
			break;
		case EBattleFuncType.EndlessHorde:
			zbWsMsgHandler.Instance.EndlessHorde_FightResult_Req(zbServerData.sGameData.EndlessHordeInfo.Level + 1, battleResult, (int)((float)zbCombatSystem.Instance.endlessHordeCurrDistance + zbCombatSystem.Instance.scrollDistance));
			break;
		case EBattleFuncType.ElementTrialElec:
		case EBattleFuncType.ElementTrialFire:
		case EBattleFuncType.ElementTrialIce:
		case EBattleFuncType.ElementTrialPhy:
		case EBattleFuncType.ElementTrialEnergy:
		case EBattleFuncType.ElementTrialWind:
			zbWsMsgHandler.Instance.ElementTrial_FightResult_Req(zbSelf.Instance.elementTrialHandler.m_attackType, zbSelf.Instance.elementTrialHandler.m_levelId, battleResult);
			break;
		case EBattleFuncType.HeroTrialQiang:
		case EBattleFuncType.HeroTrialPao:
		case EBattleFuncType.HeroTrialJiguang:
		case EBattleFuncType.HeroTrialBingdan:
		case EBattleFuncType.HeroTrialRanshaodan:
		case EBattleFuncType.HeroTrialHongzha:
		case EBattleFuncType.HeroTrialDianci:
		case EBattleFuncType.HeroTrialWurenji:
		case EBattleFuncType.HeroTrialMotuo:
		case EBattleFuncType.HeroTrialBingbao:
		case EBattleFuncType.HeroTrialShexian:
		case EBattleFuncType.HeroTrialDianqiu:
		case EBattleFuncType.HeroTrialLongjuan:
		case EBattleFuncType.HeroTrialFengren:
			zbWsMsgHandler.Instance.HeroTrial_FightResult_Req(zbServerData.sGameData.HeroTrialInfo.CurrentType, zbServerData.sGameData.HeroTrialInfo.CurrentLevel, battleResult);
			break;
		case EBattleFuncType.Legendary:
			zbWsMsgHandler.Instance.Legendary_FightResult_Req(zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId, zbSelf.Instance.legendaryHandler.playerId, battleResult);
			break;
		case EBattleFuncType.Arena:
			zbWsMsgHandler.Instance.Arena_FightResult_Req(zbSelf.Instance.arenaHandler.playerId, zbSelf.Instance.arenaHandler.rankIndex, battleResult);
			break;
		case EBattleFuncType.CrossLadderMatch:
			zbWsMsgHandler.Instance.CrossLadder_FightResult_Req(zbSelf.Instance.crossLadderHandler.playerId, battleResult);
			break;
		case EBattleFuncType.CrossLadderUpgrade:
			zbWsMsgHandler.Instance.CrossLadder_UpgradeFightResult_Req(zbSelf.Instance.crossLadderHandler.playerId, battleResult);
			break;
		case EBattleFuncType.MineWar:
			zbWsMsgHandler.Instance.MineWar_FightResult_Req(zbSelf.Instance.mineWarHandler.currPointId, zbSelf.Instance.mineWarHandler.currPlayerId, battleResult);
			break;
		case EBattleFuncType.CrossMine:
			zbWsMsgHandler.Instance.CrossMine_FightResult_Req(zbSelf.Instance.mineWarHandler.currPointId, zbSelf.Instance.mineWarHandler.currPlayerId, zbSelf.Instance.mineWarHandler.currServerId, battleResult);
			break;
		case EBattleFuncType.PeakArena:
			zbWsMsgHandler.Instance.CrossPeak_FightResult_Req(zbSelf.Instance.crossPeakHandler.currPlayerId, zbSelf.Instance.crossPeakHandler.currRank, battleResult);
			break;
		case EBattleFuncType.Truck:
			zbWsMsgHandler.Instance.Truck_RobFightResult_Req(zbSelf.Instance.truckHandler.currPlayerId, zbSelf.Instance.truckHandler.currTransportId, battleResult);
			break;
		case EBattleFuncType.LegionWar:
			zbWsMsgHandler.Instance.LegionWar_FightResult_Req(zbSelf.Instance.legionWarHandler.currPlayerId, battleResult);
			break;
		case EBattleFuncType.Facility:
			zbWsMsgHandler.Instance.Facility_FightResult_Req(zbSelf.Instance.facilityHandler.facilityMapInfo.BaseInfo.FacilityId, zbSelf.Instance.facilityHandler.curRoomId, battleResult);
			break;
		case EBattleFuncType.BossSubject:
			zbWsMsgHandler.Instance.Operate_BossSubFightResult_Req(zbSelf.Instance.bossSubjectHandler.currUUID, zbSelf.Instance.bossSubjectHandler.currBossLevel, battleResult);
			break;
		case EBattleFuncType.WallSubject:
			zbWsMsgHandler.Instance.Operate_WallSubjectFightResult_Req(zbSelf.Instance.wallSubjectHandler.currUUID, zbSelf.Instance.wallSubjectHandler.currLevelId, battleResult);
			break;
		case EBattleFuncType.MechaSubject:
			zbWsMsgHandler.Instance.Operate_MechaSubjectFightResult_Req(zbSelf.Instance.mechaSubjectHandler.currUUID, zbSelf.Instance.mechaSubjectHandler.currLevelId, battleResult);
			break;
		case EBattleFuncType.Train:
			zbWsMsgHandler.Instance.Train_RobFightResult_Req(zbSelf.Instance.truckHandler.currTransportId, battleResult);
			break;
		case EBattleFuncType.PlayerPk:
			zbWsMsgHandler.Instance.Role_FightResult_Req(zbSelf.Instance.combatHandler.roleFightPlayerId, battleResult);
			break;
		case (EBattleFuncType)6:
		case EBattleFuncType.WorldRescueDay:
		case EBattleFuncType.WorldRescueNight:
		case (EBattleFuncType)17:
		case (EBattleFuncType)18:
		case (EBattleFuncType)19:
		case (EBattleFuncType)20:
		case EBattleFuncType.GuardVerify:
		case EBattleFuncType.Season:
			break;
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	private void OnZuobiConfirm()
	{
		zbUIMgr.Instance.DestroyWnd(EWndId.Combat);
		zbUIMgr.Instance.ShowMainLevel();
		zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10667"), zbUtil.Language("lang.10668") + "_4", null);
		zbCombatSystem.Instance.combatRecorder.ClearRecord();
	}

	private void OnMsgConfirm(bool isOK)
	{
		if (isOK)
		{
			reconnectNum = 1;
			againCaclResult = 0f;
		}
		else
		{
			zbUIMgr.Instance.DestroyWnd(EWndId.Combat);
			zbUIMgr.Instance.ShowMainLevel();
		}
	}

	public override void OnUpdate()
	{
		if (curFrameConnect)
		{
			curFrameConnect = false;
		}
		else
		{
			if (reconnectNum > 3)
			{
				return;
			}
			base.OnUpdate();
			if (!zuobi)
			{
				if (!zbUIMgr.Instance.IsShow(EWndId.CombatBalance) && !zbUIMgr.Instance.IsShow(EWndId.CrossLadderBalance) && !zbUIMgr.Instance.IsShow(EWndId.TruckWinLose) && !zbUIMgr.Instance.IsShow(EWndId.LegionWarBalance) && !zbUIMgr.Instance.IsShow(EWndId.PvpBalance))
				{
					againCaclResult += Time.unscaledDeltaTime;
				}
				if (reconnectNum == 3 && againCaclResult >= 10f)
				{
					zbUIMgr.Instance.ShowMessageBox("Tip", zbUtil.Language("lang.10669"), OnMsgConfirm, zbUtil.Language("lang.10670"), zbUtil.Language("lang.10671"));
					reconnectNum = 10;
				}
				else if (againCaclResult >= 10f)
				{
					reconnectNum++;
					againCaclResult = 0f;
					Reconnect();
				}
			}
		}
	}
}
