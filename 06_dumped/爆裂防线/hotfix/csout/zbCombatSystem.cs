using System;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf.Collections;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;

public class zbCombatSystem
{
	private static zbCombatSystem sInstance;

	public BattleInfo battleInfo;

	public int paramId;

	public zbCombatRecorder restoreData;

	public zbCombatRecorder combatRecorder;

	public zbBuffSystem buffSystem;

	public zbMonsterBuffSystem monsterBuffSystem;

	public zbCombatResult combatResult;

	public zbCombatFormula combatFormula;

	public zbCombatData combatData;

	public zbCombatData combatData2;

	public zbCombatStateMachine combatStateMachine;

	public zbCombatSpeed combatSpeed;

	public zbCombatFPS combatFPS;

	public zbSafeFloat accumTime = new zbSafeFloat(0f, "zbCombatSystem.accumTime");

	public int maxTime;

	public zbSafeFloat countDown = new zbSafeFloat(0f, "zbCombatSystem.countDown");

	public float scrollSpeed;

	public float scrollTotalDistance;

	public float scrollDistance;

	public BattleResultInfo battleResultInfo;

	public RepeatedField<AwardDetail> battleAwardList = new RepeatedField<AwardDetail>();

	public static int towUUID;

	public int carAlertRange;

	public int carMaxSpeed;

	public int carAddSpeed;

	public int refreshSelectNum = 2;

	public int refreshWheelNum = 1;

	public int fightRebornNum = 1;

	public stMonster3006Reborn monster3006Reborn = new stMonster3006Reborn();

	public EBoolState isReborn;

	private string m_strFocusConfig = "";

	public int m_focusFlag;

	public zbSafeLong startServerTime = new zbSafeLong(0L, "zbCombatSystem.startServerTime");

	public bool lastRoundFlag;

	public float lastRoundTime;

	private bool m_isGotoNextLevel;

	public bool m_isCelebrating;

	public float celebrateMaxDistance = 1f;

	public float celebrateDistance;

	public float celebratePreA;

	public float celebratePreB;

	public int celebratePreTime;

	public float celebrateTimeEff;

	public int endlessHordeCurrDistance;

	public int endlessHordeNextDistance;

	public bool m_isFightForward;

	public int endlessHordeStartLevel;

	public bool m_isPingziAward;

	public BattleResultInfo dataResult;

	public bool bigBossDied;

	public bool bMonsterNeedWaterEffect;

	public bool fightFinish;

	public int qiangDamageInit;

	public long qiangHurtMin;

	public long qiangHurtMax;

	public int qiangFirstMonsterId;

	private const int kPerTidCapPVP = 1024;

	private const int kPerTidCapPVE = 256;

	private const bool kPreHeatFrameSlice = true;

	private const float kPreHeatFrameBudgetMs = 8f;

	private bool m_monsterPreHeatPending;

	private bool m_preHeatPausedByDrain;

	private float m_preHeatPreFreezeTimeScale;

	private bool m_externalPauseRequested;

	private bool m_deferredResume;

	private float m_deferredTimeScale;

	private int m_preHeatDrainFrames;

	private static readonly HashSet<int> kEffectBoneExcludedEffectIds = new HashSet<int> { 301213, 301312 };

	private static List<KeyValuePair<int, string>> s_modBulletEntries = null;

	private static readonly string[] GenericCombatPrefabPaths = new string[7] { "prefab/combat/monster.prefab", "prefab/combat/monsterEffect.prefab", "prefab/combat/monsterOnceEffect.prefab", "prefab/combat/monsterBullet.prefab", "prefab/combat/skillEffect.prefab", "prefab/combat/buffEffect.prefab", "prefab/combat/monsterDieFx.prefab" };

	public static zbCombatSystem Instance => sInstance;

	public bool isPVPBattleType { get; set; } = true;

	public bool IsMonsterPreHeatPending => m_monsterPreHeatPending;

	public static void CreateInstatnce()
	{
		sInstance = new zbCombatSystem();
	}

	public static void FreeInstatnce()
	{
		sInstance = null;
	}

	public void Init(zbCombatWnd wndCombat, BattleInfo _battleInfo, int _paramId, zbCombatRecorder _restoreData)
	{
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0777: Unknown result type (might be due to invalid IL or missing references)
		//IL_077c: Unknown result type (might be due to invalid IL or missing references)
		Time.timeScale = zbCombatSpeed.GetSpeedByEnum(ECombatSpeed.X1);
		fightFinish = false;
		m_strFocusConfig = "combatFocus_" + zbSelf.Instance.GetRoleId();
		m_focusFlag = zbGameSave.Instance.GetInt(m_strFocusConfig);
		battleInfo = _battleInfo;
		paramId = _paramId;
		restoreData = _restoreData;
		zbSelf.Instance.combatHandler.teamBaseAttrTotal = 0f;
		Transform val = wndCombat.GetMyGameObjectSceneRoot().transform.Find("CombatDataRoot");
		Transform val2 = wndCombat.GetMyGameObjectSceneRoot().transform.Find("CombatDataRoot2");
		foreach (BattleSkillDetail skill in battleInfo.AttackerTeamInfo.MasterTeamInfo.SkillList)
		{
			if (skill.SkillType != ESkillType.Qiang)
			{
				continue;
			}
			foreach (BattleSkillAttrDetail attr in skill.AttrList)
			{
				if (attr.Type == EAttrSkillType.Damage)
				{
					qiangDamageInit = (int)attr.Value;
					break;
				}
			}
			break;
		}
		qiangFirstMonsterId = battleInfo.MonsterInfo.RoundList[0].BornList[0].MonsterId;
		isPVPBattleType = battleInfo.BattleBaseInfo.BattleModel == EBattleModelType.Pvp;
		if (isPVPBattleType)
		{
			val.position = new Vector3(-50f, 0f, 0f);
			val2.position = new Vector3(50f, 0f, 0f);
			((Component)val2).gameObject.SetActive(true);
		}
		else
		{
			val.position = new Vector3(0f, 0f, 0f);
			((Component)val2).gameObject.SetActive(false);
		}
		CaclEndlessHorde();
		int num = 0;
		foreach (BattleMonsterRoundDetail round in battleInfo.MonsterInfo.RoundList)
		{
			foreach (BattleBornMonsterDetail born in round.BornList)
			{
				_ = born.MonsterId;
				_ = 1001;
				_ = born.MonsterId;
				_ = 1002;
				_ = born.MonsterId;
				_ = 1003;
			}
			num++;
		}
		foreach (BattleMonsterDetail monster in battleInfo.MonsterInfo.MonsterList)
		{
			_ = monster.MonsterId;
			_ = 1001;
			_ = monster.MonsterId;
			_ = 1002;
			_ = monster.MonsterId;
			_ = 1003;
			_ = monster.MonsterId;
			_ = 2001;
			_ = monster.MonsterId;
			_ = 3001;
		}
		startServerTime.Value = zbSelf.Instance.timeHandler.GetServerTime();
		accumTime.Value = 0f;
		maxTime = battleInfo.BattleBaseInfo.TimeLimit;
		endlessHordeStartLevel = battleInfo.BattleBaseInfo.EnemyId;
		if (IsTrialBattle(GetBattleType()))
		{
			long trialShowEndTmByBattleFuncType = zbSelf.Instance.trialHandler.GetTrialShowEndTmByBattleFuncType(Instance.battleInfo.BattleBaseInfo.BattleType);
			maxTime = Math.Min(maxTime, (int)(trialShowEndTmByBattleFuncType - zbSelf.Instance.timeHandler.GetServerTime()));
		}
		countDown.Value = maxTime;
		JObject record = zbDataConfig.Instance.gameBaseJsonTable.GetRecord();
		carAlertRange = Extensions.Value<int>((IEnumerable<JToken>)record["carSpeed"][(object)"alertRange"]);
		carMaxSpeed = Extensions.Value<int>((IEnumerable<JToken>)record["carSpeed"][(object)"maxSpeed"]);
		carAddSpeed = Extensions.Value<int>((IEnumerable<JToken>)record["carSpeed"][(object)"addSpeed"]);
		JObject record2 = zbDataConfig.Instance.endlessHordeBaseJsonTable.GetRecord();
		celebrateMaxDistance = Math.Max(Extensions.Value<float>((IEnumerable<JToken>)record2["celebrateDistance"]), 1f);
		celebrateDistance = celebrateMaxDistance;
		celebratePreA = Extensions.Value<float>((IEnumerable<JToken>)record2["celebratePreA"]);
		celebratePreB = Extensions.Value<float>((IEnumerable<JToken>)record2["celebratePreB"]);
		celebrateTimeEff = Extensions.Value<float>((IEnumerable<JToken>)record2["celebrateTimeEff"]);
		m_isCelebrating = false;
		SetRandomSeed((int)_battleInfo.BattleBaseInfo.StartTm);
		combatFPS = new zbCombatFPS();
		combatSpeed = new zbCombatSpeed();
		combatResult = new zbCombatResult();
		combatFormula = new zbCombatFormula();
		combatStateMachine = new zbCombatStateMachine();
		combatRecorder = new zbCombatRecorder();
		buffSystem = new zbBuffSystem();
		monsterBuffSystem = new zbMonsterBuffSystem();
		combatSpeed.Init();
		combatResult.Init();
		combatFormula.Init();
		combatStateMachine.Init();
		combatRecorder.Init();
		buffSystem.Init();
		monsterBuffSystem.Init();
		if (restoreData != null)
		{
			accumTime.Value = restoreData.loadedData.recordCombatSystem.accumTime;
			countDown.Value = restoreData.loadedData.recordCombatSystem.countDown;
			startServerTime.Value = restoreData.loadedData.recordCombatSystem.startServerTime;
			refreshSelectNum = restoreData.loadedData.recordCombatSystem.refreshSelectNum;
			refreshWheelNum = restoreData.loadedData.recordCombatSystem.refreshWheelNum;
			fightRebornNum = restoreData.loadedData.recordCombatSystem.fightRebornNum;
			lastRoundFlag = restoreData.loadedData.recordCombatSystem.lastRoundFlag;
			lastRoundTime = restoreData.loadedData.recordCombatSystem.lastRoundTime;
			scrollTotalDistance = restoreData.loadedData.recordCombatSystem.scrollTotalDistance;
			scrollDistance = restoreData.loadedData.recordCombatSystem.scrollDistance;
			celebrateDistance = restoreData.loadedData.recordCombatSystem.celebrateDistance;
			celebratePreTime = restoreData.loadedData.recordCombatSystem.celebratePreTime;
			monster3006Reborn.SetMonster3006Reborn(restoreData.loadedData.monster3006Reborn);
			endlessHordeStartLevel = restoreData.loadedData.recordCombatSystem.endlessHordeStartLevel;
			bigBossDied = restoreData.loadedData.recordCombatSystem.bigBossDied;
			if (monster3006Reborn.monster3006SkillSub > 0)
			{
				zbHero zbHero2 = Instance.combatData.masterTeam.heroList[0];
				zbSkillSystem.Instance.CreateSubSkill(20003, zbHero2, null, zbHero2.heroAttr.skillAttrList, new Vector3(monster3006Reborn.monster3006PositionX, monster3006Reborn.monster3006PositionY), Vector2.up);
			}
			if (zbSelf.Instance.elementTrialHandler.IsElementLairCombat(GetBattleType()))
			{
				zbSelf.Instance.elementTrialHandler.UpdateElementTrialInfo(restoreData.loadedData.elementTrial.m_attackType, restoreData.loadedData.elementTrial.m_levelId);
			}
			else if (GetBattleType() == EBattleFuncType.WorldLairHelp || GetBattleType() == EBattleFuncType.WorldLair)
			{
				zbSelf.Instance.worldLairHandler.UpdateWorldLairLevel(restoreData.loadedData.worldLair.worldLairLevel);
				if (GetBattleType() == EBattleFuncType.WorldLairHelp)
				{
					zbSelf.Instance.worldLairHandler.UpdateWorldLairUuid(restoreData.loadedData.worldLair.worldLairUuid);
				}
			}
			else if (GetBattleType() == EBattleFuncType.WorldBoss)
			{
				zbSelf.Instance.worldBossHandler.UpdateWorldBossUUid(restoreData.loadedData.worldBoss.worldBossuuid);
			}
			else if (GetBattleType() == EBattleFuncType.EliteQuest)
			{
				zbSelf.Instance.eliteQuestHandler.fightCount = restoreData.loadedData.eliteQuest.count;
				zbSelf.Instance.eliteQuestHandler.playerId = restoreData.loadedData.eliteQuest.helpId;
			}
			else if (GetBattleType() == EBattleFuncType.Legendary)
			{
				zbSelf.Instance.legendaryHandler.playerId = restoreData.loadedData.legendary.helpId;
			}
			else if (GetBattleType() == EBattleFuncType.Arena)
			{
				zbSelf.Instance.arenaHandler.playerId = restoreData.loadedData.arena.playerId;
				zbSelf.Instance.arenaHandler.rankIndex = restoreData.loadedData.arena.rankIndex;
			}
			else if (GetBattleType() == EBattleFuncType.GuardVerify)
			{
				zbSelf.Instance.guardHandler.CombatIndex = restoreData.loadedData.guardVerify.CombatIndex;
			}
			else if (GetBattleType() == EBattleFuncType.CrossLadderMatch || GetBattleType() == EBattleFuncType.CrossLadderUpgrade)
			{
				zbSelf.Instance.crossLadderHandler.playerId = restoreData.loadedData.crossLadder.playerId;
			}
			else if (GetBattleType() == EBattleFuncType.MineWar || GetBattleType() == EBattleFuncType.CrossMine)
			{
				zbSelf.Instance.mineWarHandler.currPointId = restoreData.loadedData.mineWar.currPointId;
				zbSelf.Instance.mineWarHandler.currPlayerId = restoreData.loadedData.mineWar.currPlayerId;
				zbSelf.Instance.mineWarHandler.currServerId = restoreData.loadedData.mineWar.currServerId;
			}
			else if (GetBattleType() == EBattleFuncType.PeakArena)
			{
				zbSelf.Instance.crossPeakHandler.currPlayerId = restoreData.loadedData.peakArena.currPlayerId;
				zbSelf.Instance.crossPeakHandler.currRank = restoreData.loadedData.peakArena.currRank;
			}
			else if (GetBattleType() == EBattleFuncType.Truck || GetBattleType() == EBattleFuncType.Train)
			{
				zbSelf.Instance.truckHandler.currTransportId = restoreData.loadedData.truck.currTransportId;
				zbSelf.Instance.truckHandler.currPlayerId = restoreData.loadedData.truck.currPlayerId;
			}
			else if (GetBattleType() == EBattleFuncType.LegionWar)
			{
				zbSelf.Instance.legionWarHandler.currPlayerId = restoreData.loadedData.legionWar.currPlayerId;
			}
			else if (GetBattleType() == EBattleFuncType.BossSubject)
			{
				zbSelf.Instance.bossSubjectHandler.currUUID = restoreData.loadedData.bossSubject.currUUID;
				zbSelf.Instance.bossSubjectHandler.currBossLevel = restoreData.loadedData.bossSubject.currBossLevel;
				zbSelf.Instance.bossSubjectHandler.currDamageAdd = restoreData.loadedData.bossSubject.currDamageAdd;
			}
			else if (GetBattleType() == EBattleFuncType.WallSubject)
			{
				zbSelf.Instance.wallSubjectHandler.currUUID = restoreData.loadedData.wallSubject.currUUID;
				zbSelf.Instance.wallSubjectHandler.currLevelId = restoreData.loadedData.wallSubject.currLevelId;
			}
			else if (GetBattleType() == EBattleFuncType.MechaSubject)
			{
				zbSelf.Instance.mechaSubjectHandler.currUUID = restoreData.loadedData.mechaSubject.currUUID;
				zbSelf.Instance.mechaSubjectHandler.currLevelId = restoreData.loadedData.mechaSubject.currLevelId;
			}
		}
		combatData = new zbCombatData();
		combatData.Init(val, battleInfo.AttackerTeamInfo, 1);
		if (isPVPBattleType)
		{
			combatData2 = new zbCombatData();
			combatData2.Init(val2, battleInfo.DefenderTeamInfo, 2);
		}
		zbPerfLog.Reset();
		zbCombatData snapshotData1 = combatData;
		zbCombatData snapshotData2 = combatData2;
		zbPerfLog.SnapshotProvider = delegate
		{
			string text = ((snapshotData1 != null) ? snapshotData1.GetSpineTierSnapshot() : null);
			string text2 = ((snapshotData2 != null) ? snapshotData2.GetSpineTierSnapshot() : null);
			return (text != null && text2 != null) ? (text + "；" + text2) : (text ?? text2);
		};
		if (restoreData != null)
		{
			combatData.Restore(restoreData);
			if (combatData2 != null)
			{
				combatData2.Restore(restoreData);
			}
		}
		else
		{
			combatRecorder.ClearRecord();
			combatData.UnRestoreData();
			if (combatData2 != null)
			{
				combatData2.UnRestoreData();
			}
		}
		combatStateMachine.SetState(ECombatState.Ready);
		CheckIsGotoNextLevel();
	}

	public void CaclEndlessHorde()
	{
		zbSelf.Instance.combatHandler.RecordBattleInfo(battleInfo);
		if (GetBattleType() == EBattleFuncType.EndlessHorde)
		{
			endlessHordeCurrDistance = zbSelf.Instance.endlessHordeHandler.GetDistance(zbServerData.sGameData.EndlessHordeInfo.Level);
			endlessHordeNextDistance = zbSelf.Instance.endlessHordeHandler.GetDistance(zbServerData.sGameData.EndlessHordeInfo.Level + 1);
		}
		scrollDistance = 0f;
	}

	public void Free()
	{
		Time.timeScale = zbCombatSpeed.GetSpeedByEnum(ECombatSpeed.X1);
		m_monsterPreHeatPending = false;
		m_preHeatPausedByDrain = false;
		m_deferredResume = false;
		m_deferredTimeScale = 0f;
		zbPerfLog.SnapshotProvider = null;
		zbPerfLog.Reset();
		zbCombatAtlasMerger.Instance.Clear();
		zbMonsterSpineMaterialUtil.ClearCache();
		zbSpineGpuAnimRuntime.Clear();
		zbCombatSpineAssetUtil.Clear();
		buffSystem.Free();
		monsterBuffSystem.Free();
		combatResult.Free();
		combatFormula.Free();
		combatStateMachine.Free();
		combatData.Free();
		if (combatData2 != null)
		{
			combatData2.Free();
		}
		combatRecorder.Free();
	}

	private void BeginMonsterPreHeatDrain()
	{
		if (combatData.monsterPoolMgr.HasPendingPreHeat || (combatData2 != null && combatData2.monsterPoolMgr.HasPendingPreHeat))
		{
			m_monsterPreHeatPending = true;
			m_preHeatDrainFrames = 0;
			m_preHeatPausedByDrain = false;
			m_externalPauseRequested = false;
			m_deferredResume = false;
			m_deferredTimeScale = 0f;
			if (Time.timeScale > 0.01f)
			{
				m_preHeatPausedByDrain = true;
				m_preHeatPreFreezeTimeScale = Time.timeScale;
				Time.timeScale = 0f;
			}
			zbPerfLog.Log("[monsterPool] 分帧预热开始: 自方入队=" + combatData.monsterPoolMgr.PendingPreHeatTotal + ((combatData2 != null) ? (" 敌方入队=" + combatData2.monsterPoolMgr.PendingPreHeatTotal) : "") + " 预算=" + 8f.ToString("0") + "ms/帧");
		}
	}

	private void UpdateMonsterPreHeatDrainFrame()
	{
		m_preHeatDrainFrames++;
		Stopwatch watch = Stopwatch.StartNew();
		combatData.monsterPoolMgr.DrainPreHeat(watch, 8f);
		if (combatData2 != null)
		{
			combatData2.monsterPoolMgr.DrainPreHeat(watch, 8f);
		}
		if (!combatData.monsterPoolMgr.HasPendingPreHeat && (combatData2 == null || !combatData2.monsterPoolMgr.HasPendingPreHeat))
		{
			m_monsterPreHeatPending = false;
			if (m_deferredTimeScale > 0f)
			{
				Time.timeScale = m_deferredTimeScale;
			}
			else if (m_deferredResume)
			{
				Resume();
			}
			else if (m_preHeatPausedByDrain && !m_externalPauseRequested)
			{
				Time.timeScale = m_preHeatPreFreezeTimeScale;
			}
			m_preHeatPausedByDrain = false;
			m_externalPauseRequested = false;
			m_deferredResume = false;
			m_deferredTimeScale = 0f;
			string text = ((combatData2 != null) ? combatData2.monsterPoolMgr.PreHeatStatLine() : null);
			zbPerfLog.Log("[monsterPool] 分帧预热完成: 帧数=" + m_preHeatDrainFrames + " 自方=" + combatData.monsterPoolMgr.PreHeatStatLine() + ((text != null) ? (" 敌方=" + text) : ""));
		}
	}

	public void SetTimeScaleOrDeferPreHeat(float scale)
	{
		if (m_monsterPreHeatPending)
		{
			m_deferredTimeScale = scale;
		}
		else
		{
			Time.timeScale = scale;
		}
	}

	public void Update()
	{
		if (m_monsterPreHeatPending)
		{
			zbPerfLog.TickFrame();
			UpdateMonsterPreHeatDrainFrame();
			return;
		}
		accumTime.Value += Time.deltaTime;
		zbPerfLog.TickFrame();
		UpdateCountdown();
		combatFPS.Update();
		buffSystem.Update();
		monsterBuffSystem.Update();
		combatData.Update();
		if (combatData2 != null)
		{
			combatData2.Update();
		}
		combatStateMachine.Update();
		combatRecorder.Update();
		if (IsTimeOver() || IsKillAll() || IsDefenseHpZero() || IsKillBoss())
		{
			if (zbUIMgr.Instance.IsShow(EWndId.WorldBossRebron))
			{
				return;
			}
			if (GetBattleType() == EBattleFuncType.WorldBoss && fightRebornNum > 0 && IsDefenseHpZero())
			{
				zbUIMgr.Instance.ShowWorldBossReborn();
				return;
			}
			fightFinish = true;
			combatStateMachine.SetState(ECombatState.Balance);
		}
		if (GetDefenseType() == EDefenseType.CarDefense)
		{
			zbMonster closestBossMonster = combatData.monsters.GetClosestBossMonster(10f);
			if ((Object)(object)closestBossMonster != (Object)null && m_isCelebrating)
			{
				m_isCelebrating = false;
				((Component)(zbUIMgr.Instance.GetWndById(EWndId.Combat) as zbCombatWnd).m_endlessHordePanel.m_CelebrateAnim).gameObject.SetActive(false);
				scrollSpeed = (float)carMaxSpeed / 100f;
			}
			if (m_isCelebrating && (Object)(object)closestBossMonster == (Object)null)
			{
				scrollSpeed = 5f;
			}
			else if (combatData.monsters.HasMonsterInAlertRange((float)carAlertRange / 100f) || (Object)(object)closestBossMonster != (Object)null)
			{
				scrollSpeed = Math.Max(scrollSpeed - Time.deltaTime * (float)carAddSpeed / 100f, 0f);
			}
			else
			{
				scrollSpeed = Math.Min(scrollSpeed + Time.deltaTime * (float)carAddSpeed / 100f, (float)carMaxSpeed / 100f);
			}
		}
	}

	public void UpdateCountdown()
	{
		if (maxTime > 0)
		{
			if (GetBattleType() == EBattleFuncType.LegionWar)
			{
				countDown.Value = Math.Max((float)countDown - Time.unscaledDeltaTime, 0f);
			}
			else
			{
				countDown.Value = Math.Max((float)countDown - Time.deltaTime, 0f);
			}
		}
		else
		{
			countDown.Value += Time.deltaTime;
		}
	}

	public bool IsTimeOver()
	{
		if (maxTime > 0 && (float)countDown <= 0f)
		{
			return true;
		}
		return false;
	}

	public bool IsKillAll()
	{
		if (!combatData.combatExp.IsKillAll())
		{
			if (combatData2 != null)
			{
				return combatData2.combatExp.IsKillAll();
			}
			return false;
		}
		return true;
	}

	public bool IsDefenseHpZero()
	{
		if ((long)combatData.defense.curHp > 0)
		{
			if (combatData2 != null)
			{
				return (long)combatData2.defense.curHp <= 0;
			}
			return false;
		}
		return true;
	}

	public bool IsKillBoss()
	{
		if (GetBattleModel() == EBattleModelType.KillBoss && bigBossDied)
		{
			return true;
		}
		return false;
	}

	public void SetRandomSeed(int seed)
	{
		Rand.InitState(seed);
	}

	public void Pause()
	{
		if (m_monsterPreHeatPending)
		{
			m_externalPauseRequested = true;
		}
		Time.timeScale = 0f;
	}

	public void Resume()
	{
		if (m_monsterPreHeatPending)
		{
			m_deferredResume = true;
			return;
		}
		ECombatSpeed speed = combatSpeed.GetSpeed();
		combatSpeed.SetSpeed(speed);
		Instance.combatRecorder.Save();
	}

	public bool IsPaused()
	{
		return Time.timeScale <= 0.01f;
	}

	public bool CanPause()
	{
		return true;
	}

	public EBattleFuncType GetBattleType()
	{
		return battleInfo.BattleBaseInfo.BattleType;
	}

	public EBattleModelType GetBattleModel()
	{
		return battleInfo.BattleBaseInfo.BattleModel;
	}

	public EDefenseType GetDefenseType()
	{
		return battleInfo.AttackerTeamInfo.MasterTeamInfo.DefenseInfo.DefenseType;
	}

	public string GetBattleUUID()
	{
		return battleInfo.BattleBaseInfo.UUID;
	}

	public zbBaseWnd SetBanlanceData(BattleResultInfo _BattleResultInfo)
	{
		battleResultInfo = _BattleResultInfo;
		GetBalanceWndInfo(out var wndId, out var prefabPath);
		zbBaseWnd zbBaseWnd2 = zbUIMgr.Instance.ShowWnd(wndId, prefabPath);
		if (wndId == EWndId.PvpBalance && (Object)(object)zbBaseWnd2 != (Object)null)
		{
			(zbBaseWnd2 as zbPvpBalanceWnd)?.RefreshCustom();
		}
		(zbUIMgr.Instance.GetWndById(EWndId.Combat) as zbCombatWnd)?.FreezeBattleCameras();
		return zbBaseWnd2;
	}

	private void GetBalanceWndInfo(out EWndId wndId, out string prefabPath)
	{
		switch ((battleResultInfo != null) ? battleResultInfo.BattleType : battleInfo.BattleBaseInfo.BattleType)
		{
		case EBattleFuncType.CrossLadderMatch:
		case EBattleFuncType.CrossLadderUpgrade:
			wndId = EWndId.CrossLadderBalance;
			prefabPath = "prefab/wnd/crossLadder/crossLadderBalance.prefab";
			return;
		case EBattleFuncType.LegionWar:
			wndId = EWndId.LegionWarBalance;
			prefabPath = "prefab/wnd/legionWar/legionWarBalance.prefab";
			return;
		}
		if (isPVPBattleType)
		{
			wndId = EWndId.PvpBalance;
			prefabPath = "prefab/wnd/pvp/pvpBalance.prefab";
		}
		else
		{
			wndId = EWndId.CombatBalance;
			prefabPath = "prefab/wnd/combat/combatBalance.prefab";
		}
	}

	public void PreHeatBattleAssets()
	{
		GetBalanceWndInfo(out var wndId, out var prefabPath);
		zbUIMgr.Instance.PreloadWnd(wndId, prefabPath);
		PreHeatTeamSkills(combatData);
		if (combatData2 != null)
		{
			PreHeatTeamSkills(combatData2);
		}
		if (isPVPBattleType)
		{
			PreHeatMonsterEffect(combatData);
			if (combatData2 != null)
			{
				PreHeatMonsterEffect(combatData2);
			}
		}
		PreHeatGenericCombatPrefabs();
		PreHeatLevelMonsterSpine();
		PreHeatMonsterPools(combatData);
		if (combatData2 != null)
		{
			PreHeatMonsterPools(combatData2);
		}
		PreHeatEffectPools(combatData);
		if (combatData2 != null)
		{
			PreHeatEffectPools(combatData2);
		}
		BeginMonsterPreHeatDrain();
	}

	private void PreHeatEffectPools(zbCombatData data)
	{
		if (data != null)
		{
			data.monsterEyeEffectPoolMgr.PreHeat(8);
			data.monsterDieEffectPoolMgr.PreHeat(16);
			data.monsterHitEffectPoolMgr.PreHeat(16);
		}
	}

	private void PreHeatMonsterPools(zbCombatData data)
	{
		if (data == null || battleInfo == null || battleInfo.MonsterInfo == null)
		{
			return;
		}
		int val = (isPVPBattleType ? 1024 : 256);
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (BattleMonsterRoundDetail round in battleInfo.MonsterInfo.RoundList)
		{
			if (round == null)
			{
				continue;
			}
			Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
			foreach (BattleBornMonsterDetail born in round.BornList)
			{
				if (born != null && born.Num > 0)
				{
					dictionary2.TryGetValue(born.MonsterId, out var value);
					dictionary2[born.MonsterId] = value + born.Num;
				}
			}
			foreach (KeyValuePair<int, int> item in dictionary2)
			{
				dictionary.TryGetValue(item.Key, out var value2);
				if (item.Value > value2)
				{
					dictionary[item.Key] = item.Value;
				}
			}
		}
		int num = 0;
		foreach (KeyValuePair<int, int> item2 in dictionary)
		{
			int num2 = Math.Min(item2.Value, val);
			if (num2 > 0)
			{
				if (data.monsterPoolMgr.EnqueuePreHeat(item2.Key, num2))
				{
					zbPerfLog.Log("[monsterPool] 预热入队: tid=" + item2.Key + " 单轮max=" + item2.Value + " 入队=" + num2 + " cap=" + val + ((data == combatData2) ? "(敌侧)" : ""));
				}
				num += num2;
				data.monsterBulletEffectPoolMgr.PreHeat(item2.Key, Math.Min(8, Math.Max(2, item2.Value / 2)));
			}
		}
		data.monsterDieFxPool.PreHeat(Math.Max(8, Math.Min(num, 32)));
	}

	private void PreHeatLevelMonsterSpine()
	{
		if (battleInfo == null || battleInfo.MonsterInfo == null)
		{
			return;
		}
		List<string> list = new List<string>();
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		Dictionary<int, string> dictionary2 = new Dictionary<int, string>();
		HashSet<int> hashSet = new HashSet<int>();
		foreach (BattleMonsterRoundDetail round in battleInfo.MonsterInfo.RoundList)
		{
			if (round == null)
			{
				continue;
			}
			foreach (BattleBornMonsterDetail born in round.BornList)
			{
				if (born == null)
				{
					continue;
				}
				dictionary.TryGetValue(born.MonsterId, out var value);
				dictionary[born.MonsterId] = value + born.Num;
				if (!hashSet.Add(born.MonsterId))
				{
					continue;
				}
				PreHeatMonsterSpine(born.MonsterId);
				zbMonsterCsvRecord recordByKey = zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(born.MonsterId);
				string text = ((recordByKey != null) ? ((EMonsterType)recordByKey.type/*cast due to constrained. prefix*/).ToString() : "配置缺失");
				if (recordByKey != null && recordByKey.type == 1)
				{
					zbMonsterAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.monsterAvatarCsvTable.GetRecordByKey(recordByKey.avatarId);
					if (recordByKey2 != null && !string.IsNullOrEmpty(recordByKey2.fightBone))
					{
						string text2 = recordByKey2.fightBone + "_SkeletonData.asset";
						list.Add(text2);
						dictionary2[born.MonsterId] = "入合并收集 path=" + text2;
					}
					else
					{
						dictionary2[born.MonsterId] = "排除：avatar 配置缺失（无 fightBone）";
					}
				}
				else
				{
					dictionary2[born.MonsterId] = "排除：非普通怪（" + text + "）";
				}
			}
		}
		int num = 0;
		foreach (int value3 in dictionary.Values)
		{
			num += value3;
		}
		foreach (KeyValuePair<int, string> item in dictionary2)
		{
			dictionary.TryGetValue(item.Key, out var _);
		}
		zbSpineGpuAnimRuntime.AppendSummonDerivedPaths(list, hashSet);
		Stopwatch stopwatch = Stopwatch.StartNew();
		zbCombatAtlasMerger.Instance.Prepare(list);
		double totalMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
		stopwatch.Restart();
		zbSpineGpuAnimRuntime.Prepare(list);
		zbPerfLog.Log($"[zbCombat] PreHeat spine 链路：合并器 {totalMilliseconds:0.#} ms + GPU烘焙 {stopwatch.Elapsed.TotalMilliseconds:0.#} ms");
		PreHeatLevelEffectSpine();
	}

	private void PreHeatLevelEffectSpine()
	{
		if (!zbSpineGpuAnimRuntime.kEnabled)
		{
			return;
		}
		List<string> list = CollectEffectSpinePaths(out var lowPriorityHeroPaths);
		if (list.Count == 0)
		{
			return;
		}
		List<string> list2 = CollectModBulletLowPriorityPaths();
		foreach (string item in lowPriorityHeroPaths)
		{
			if (!list2.Contains(item))
			{
				list2.Add(item);
			}
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		zbCombatAtlasMerger.Instance.PrepareEffect(list, list2);
		double totalMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
		stopwatch.Restart();
		if (list2.Count > 0)
		{
			List<string> list3 = new List<string>(list.Count + list2.Count);
			list3.AddRange(list);
			foreach (string item2 in list2)
			{
				if (!list.Contains(item2))
				{
					list3.Add(item2);
				}
			}
			zbSpineGpuAnimRuntime.PrepareEffect(list3);
		}
		else
		{
			zbSpineGpuAnimRuntime.PrepareEffect(list);
		}
		zbPerfLog.Log(string.Format("[zbCombat] PreHeat 特效 spine 链路：候选 {0} 型（+词条低优先 {1} 型），合并器 {2:0.#} ms + GPU烘焙 {3:0.#} ms", new object[4]
		{
			list.Count,
			list2.Count,
			totalMilliseconds,
			stopwatch.Elapsed.TotalMilliseconds
		}));
	}

	private static List<string> CollectModBulletLowPriorityPaths()
	{
		List<string> list = new List<string>();
		zbCombatData zbCombatData2 = Instance?.combatData;
		if (zbCombatData2 == null)
		{
			return list;
		}
		HashSet<int> skillTypes = new HashSet<int>();
		CollectTeamSkillTypes(zbCombatData2.masterTeam, skillTypes);
		CollectTeamSkillTypes(zbCombatData2.helperTeam, skillTypes);
		GetModBulletSpinePaths(skillTypes, list);
		return list;
	}

	private static void CollectTeamSkillTypes(zbCombatTeam team, HashSet<int> skillTypes)
	{
		if (team == null)
		{
			return;
		}
		if (team.heroList != null)
		{
			foreach (zbHero hero in team.heroList)
			{
				if ((Object)(object)hero != (Object)null)
				{
					skillTypes.Add(hero.GetSkillType());
				}
			}
		}
		if (team.battleTeamInfo == null || team.battleTeamInfo.SkillList == null)
		{
			return;
		}
		foreach (BattleSkillDetail skill in team.battleTeamInfo.SkillList)
		{
			if (skill != null)
			{
				skillTypes.Add((int)skill.SkillType);
			}
		}
	}

	private static List<string> CollectEffectSpinePaths(out List<string> lowPriorityHeroPaths)
	{
		lowPriorityHeroPaths = new List<string>();
		HashSet<string> hashSet = new HashSet<string>();
		zbDataConfig instance = zbDataConfig.Instance;
		if (instance == null)
		{
			return new List<string>();
		}
		bool flag = Instance != null && Instance.isPVPBattleType;
		zbMonsterEffectCsvTableData zbMonsterEffectCsvTableData2 = ((instance.monsterEffectCsvTable != null) ? instance.monsterEffectCsvTable.GetRecords() : null);
		if (zbMonsterEffectCsvTableData2 != null && zbMonsterEffectCsvTableData2.records != null)
		{
			foreach (KeyValuePair<int, zbMonsterEffectCsvRecord> record in zbMonsterEffectCsvTableData2.records)
			{
				zbMonsterEffectCsvRecord value = record.Value;
				if (value != null && !string.IsNullOrEmpty(value.spinePath) && !kEffectBoneExcludedEffectIds.Contains(record.Key) && (flag || (record.Key != 1 && record.Key != 2)))
				{
					hashSet.Add(value.spinePath + "_SkeletonData.asset");
				}
			}
		}
		zbSkillBuffCsvTableData zbSkillBuffCsvTableData2 = ((instance.skillBuffCsvTable != null) ? instance.skillBuffCsvTable.GetRecords() : null);
		if (zbSkillBuffCsvTableData2 != null && zbSkillBuffCsvTableData2.records != null)
		{
			foreach (KeyValuePair<int, zbSkillBuffCsvRecord> record2 in zbSkillBuffCsvTableData2.records)
			{
				zbSkillBuffCsvRecord value2 = record2.Value;
				if (value2 != null && !string.IsNullOrEmpty(value2.spinePath))
				{
					hashSet.Add(value2.spinePath + "_SkeletonData.asset");
				}
			}
		}
		zbSkillEffectCsvTableData zbSkillEffectCsvTableData2 = ((instance.skillEffectCsvTable != null) ? instance.skillEffectCsvTable.GetRecords() : null);
		if (zbSkillEffectCsvTableData2 != null && zbSkillEffectCsvTableData2.records != null)
		{
			foreach (KeyValuePair<int, zbSkillEffectCsvRecord> record3 in zbSkillEffectCsvTableData2.records)
			{
				zbSkillEffectCsvRecord value3 = record3.Value;
				if (value3 != null && !string.IsNullOrEmpty(value3.spinePath))
				{
					hashSet.Add(value3.spinePath + "_SkeletonData.asset");
				}
			}
		}
		CollectHeroBulletSpinePaths(hashSet, lowPriorityHeroPaths);
		zbCombatData zbCombatData2 = Instance?.combatData;
		if (zbCombatData2 != null && zbCombatData2.GetSceneAlertRange() < 10f)
		{
			hashSet.Add("animation/effect/monster/monster_eye/monster_eye_SkeletonData.asset");
		}
		return new List<string>(hashSet);
	}

	private static void CollectHeroBulletSpinePaths(HashSet<string> paths, List<string> lowPriorityPaths)
	{
		zbCombatData zbCombatData2 = Instance?.combatData;
		if (zbCombatData2 != null)
		{
			CollectTeamBulletPaths(zbCombatData2.masterTeam, paths, lowPriorityPaths);
			CollectTeamBulletPaths(zbCombatData2.helperTeam, paths, lowPriorityPaths);
		}
	}

	private static void CollectTeamBulletPaths(zbCombatTeam team, HashSet<string> paths, List<string> lowPriorityPaths)
	{
		if (team == null)
		{
			return;
		}
		HashSet<int> hashSet = new HashSet<int>();
		if (team.heroList != null)
		{
			foreach (zbHero hero in team.heroList)
			{
				if (!((Object)(object)hero == (Object)null) && hero.skillRecord != null)
				{
					hashSet.Add(hero.GetSkillType());
					if (!string.IsNullOrEmpty(hero.skillRecord.bulletSpineName))
					{
						AddHeroBulletPath(hero.skillRecord.bulletSpineName, paths);
					}
				}
			}
		}
		if (team.battleTeamInfo != null && team.battleTeamInfo.RoleBaseInfo != null)
		{
			zbPlayerGunCsvRecord zbPlayerGunCsvRecord2 = ((zbDataConfig.Instance != null && zbDataConfig.Instance.playerGunCsvTable != null) ? zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(team.battleTeamInfo.RoleBaseInfo.GunId) : null);
			if (zbPlayerGunCsvRecord2 != null)
			{
				if (TryGetModBulletPath(zbPlayerGunCsvRecord2.mod, out var path, out var skillType) && !string.IsNullOrEmpty(path))
				{
					paths.Add(path);
					if (skillType > 0)
					{
						hashSet.Add(skillType);
					}
				}
				if (TryGetModBulletPath(zbPlayerGunCsvRecord2.mod2, out var path2, out var skillType2) && !string.IsNullOrEmpty(path2))
				{
					paths.Add(path2);
					if (skillType2 > 0)
					{
						hashSet.Add(skillType2);
					}
				}
			}
		}
		if (team.battleTeamInfo == null || team.battleTeamInfo.SkillList == null)
		{
			return;
		}
		zbSkillCsvTableData zbSkillCsvTableData2 = ((zbDataConfig.Instance != null && zbDataConfig.Instance.skillCsvTable != null) ? zbDataConfig.Instance.skillCsvTable.GetRecords() : null);
		if (zbSkillCsvTableData2 == null)
		{
			return;
		}
		foreach (BattleSkillDetail skill in team.battleTeamInfo.SkillList)
		{
			if (skill == null || !zbSkillCsvTableData2.records.TryGetValue(skill.SkillId, out var value) || value == null || string.IsNullOrEmpty(value.bulletSpineName))
			{
				continue;
			}
			if (hashSet.Contains((int)skill.SkillType))
			{
				AddHeroBulletPath(value.bulletSpineName, paths);
				continue;
			}
			string item = "animation/effect/hero/" + value.bulletSpineName + "/" + value.bulletSpineName + "_SkeletonData.asset";
			if (!lowPriorityPaths.Contains(item))
			{
				lowPriorityPaths.Add(item);
			}
		}
	}

	private static bool TryGetModBulletPath(int modId, out string path, out int skillType)
	{
		path = null;
		skillType = 0;
		if (modId <= 0)
		{
			return false;
		}
		zbModCsvTableData zbModCsvTableData2 = ((zbDataConfig.Instance != null && zbDataConfig.Instance.modCsvTable != null) ? zbDataConfig.Instance.modCsvTable.GetRecords() : null);
		zbSkillCsvTableData zbSkillCsvTableData2 = ((zbDataConfig.Instance != null && zbDataConfig.Instance.skillCsvTable != null) ? zbDataConfig.Instance.skillCsvTable.GetRecords() : null);
		if (zbModCsvTableData2 == null || zbModCsvTableData2.records == null || zbSkillCsvTableData2 == null || zbSkillCsvTableData2.records == null)
		{
			return false;
		}
		if (!zbModCsvTableData2.records.TryGetValue(modId, out var value) || value == null || value.modIndex != 1002 || value.clientEff == null || value.clientEff.Length == 0)
		{
			return false;
		}
		if (value.skillType != null && value.skillType.Length != 0)
		{
			skillType = value.skillType[0];
		}
		if (zbSkillCsvTableData2.records.TryGetValue((int)value.clientEff[0], out var value2) && value2 != null && !string.IsNullOrEmpty(value2.bulletSpineName))
		{
			path = "animation/effect/hero/" + value2.bulletSpineName + "/" + value2.bulletSpineName + "_SkeletonData.asset";
			return true;
		}
		return false;
	}

	private static void AddHeroBulletPath(string bulletSpineName, HashSet<string> paths)
	{
		paths.Add("animation/effect/hero/" + bulletSpineName + "/" + bulletSpineName + "_SkeletonData.asset");
	}

	private static void GetModBulletSpinePaths(HashSet<int> skillTypes, List<string> results)
	{
		if (s_modBulletEntries == null)
		{
			List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
			zbModCsvTableData zbModCsvTableData2 = ((zbDataConfig.Instance != null && zbDataConfig.Instance.modCsvTable != null) ? zbDataConfig.Instance.modCsvTable.GetRecords() : null);
			zbSkillCsvTableData zbSkillCsvTableData2 = ((zbDataConfig.Instance != null && zbDataConfig.Instance.skillCsvTable != null) ? zbDataConfig.Instance.skillCsvTable.GetRecords() : null);
			if (zbModCsvTableData2 != null && zbModCsvTableData2.records != null && zbSkillCsvTableData2 != null && zbSkillCsvTableData2.records != null)
			{
				HashSet<string> hashSet = new HashSet<string>();
				foreach (KeyValuePair<int, zbModCsvRecord> record in zbModCsvTableData2.records)
				{
					zbModCsvRecord value = record.Value;
					if (value == null || value.modIndex != 1002 || value.clientEff == null || value.clientEff.Length == 0)
					{
						continue;
					}
					int key = ((value.skillType != null && value.skillType.Length != 0) ? value.skillType[0] : 0);
					if (zbSkillCsvTableData2.records.TryGetValue((int)value.clientEff[0], out var value2) && value2 != null && !string.IsNullOrEmpty(value2.bulletSpineName))
					{
						string text = "animation/effect/hero/" + value2.bulletSpineName + "/" + value2.bulletSpineName + "_SkeletonData.asset";
						if (hashSet.Add(text))
						{
							list.Add(new KeyValuePair<int, string>(key, text));
						}
					}
				}
			}
			s_modBulletEntries = list;
			zbPerfLog.Log($"[zbCombat] 1002 换技能词条弹型全集：{list.Count} 型（首次解析，跨场缓存）");
		}
		foreach (KeyValuePair<int, string> s_modBulletEntry in s_modBulletEntries)
		{
			if (s_modBulletEntry.Key == 0 || skillTypes.Contains(s_modBulletEntry.Key))
			{
				results.Add(s_modBulletEntry.Value);
			}
		}
	}

	private static void PreHeatMonsterSpine(int monsterTid)
	{
		zbMonsterCsvRecord recordByKey = zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(monsterTid);
		if (recordByKey != null)
		{
			zbMonsterAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.monsterAvatarCsvTable.GetRecordByKey(recordByKey.avatarId);
			if (recordByKey2 != null && !string.IsNullOrEmpty(recordByKey2.fightBone))
			{
				zbUtil.PreheatSkeletonData(recordByKey2.fightBone + "_SkeletonData.asset");
			}
		}
	}

	private void PreHeatGenericCombatPrefabs()
	{
		string[] genericCombatPrefabPaths = GenericCombatPrefabPaths;
		for (int i = 0; i < genericCombatPrefabPaths.Length; i++)
		{
			zbUtil.PreheatPrefabSkeletonData(genericCombatPrefabPaths[i]);
		}
	}

	private void PreHeatTeamSkills(zbCombatData data)
	{
		if (data != null)
		{
			PreHeatHeroListSkills(data, data.masterTeam.heroList);
			if (data.helperTeam != null)
			{
				PreHeatHeroListSkills(data, data.helperTeam.heroList);
			}
		}
	}

	private void PreHeatHeroListSkills(zbCombatData data, List<zbHero> heroList)
	{
		foreach (zbHero hero in heroList)
		{
			data.skillPoolMgr.PreHeatPool(hero.skillTypeRecord.bulletPrefab, 2);
		}
	}

	private void PreHeatMonsterEffect(zbCombatData data)
	{
		PreHeatEffectRecord(data.dataIndex);
		PreHeatEffectRecord(data.dataIndex + 10);
	}

	private void PreHeatEffectRecord(int monsterEffectId)
	{
		zbMonsterEffectCsvRecord recordByKey = zbDataConfig.Instance.monsterEffectCsvTable.GetRecordByKey(monsterEffectId);
		if (recordByKey != null && !string.IsNullOrEmpty(recordByKey.spinePath))
		{
			zbUtil.PreheatSkeletonData(recordByKey.spinePath + "_SkeletonData.asset");
		}
	}

	public bool IsTrialBattle(EBattleFuncType battleFuncType)
	{
		return zbSelf.Instance.trialHandler.GetSkillType(battleFuncType) != ESkillType.EmptyValue;
	}

	public bool IsWorldLair()
	{
		if (battleInfo.BattleBaseInfo.BattleType != EBattleFuncType.WorldLair)
		{
			return battleInfo.BattleBaseInfo.BattleType == EBattleFuncType.WorldLairHelp;
		}
		return true;
	}

	public bool isLegionBoss()
	{
		return battleInfo.BattleBaseInfo.BattleType == EBattleFuncType.LegionBoss;
	}

	public bool isWorldBoss()
	{
		return battleInfo.BattleBaseInfo.BattleType == EBattleFuncType.WorldBoss;
	}

	public void SetFocus(int _focusFlag)
	{
		m_focusFlag = Math.Clamp(_focusFlag, 0, 1);
		zbGameSave.Instance.SetInt(m_strFocusConfig, m_focusFlag);
	}

	private void CheckIsGotoNextLevel()
	{
		m_isGotoNextLevel = false;
		switch (Instance.battleInfo.BattleBaseInfo.BattleType)
		{
		case EBattleFuncType.MainQuest:
			if (Instance.battleInfo.BattleBaseInfo.EnemyId == zbServerData.sGameData.MainQuestInfo.NextLevel)
			{
				m_isGotoNextLevel = true;
			}
			break;
		case EBattleFuncType.EliteQuest:
			if (Instance.battleInfo.BattleBaseInfo.EnemyId == zbServerData.sGameData.EliteQuestInfo.NextLevel)
			{
				m_isGotoNextLevel = true;
			}
			break;
		}
	}

	public bool IsGotoNextLevel()
	{
		return m_isGotoNextLevel;
	}
}
