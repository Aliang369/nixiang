using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Proto.Enums;
using UnityEngine;

public class zbCombatRecorder
{
	private class zbCombatRecordSaveOp
	{
		public stCombatRecorderData snapshot;
	}

	private class zbCombatRecordSaveWorker
	{
		private readonly object m_gate = new object();

		private readonly List<zbCombatRecordSaveOp> m_ops = new List<zbCombatRecordSaveOp>();

		private readonly Action<zbCombatRecordSaveOp> m_execute;

		private Thread m_thread;

		private bool m_stopping;

		private bool m_executing;

		private zbCombatRecordSaveWorker(Action<zbCombatRecordSaveOp> execute)
		{
			m_execute = execute;
		}

		public static zbCombatRecordSaveWorker TryCreate(Action<zbCombatRecordSaveOp> execute)
		{
			try
			{
				ProbeThreadingCapabilities();
				zbCombatRecordSaveWorker obj = new zbCombatRecordSaveWorker(execute);
				obj.m_thread = new Thread(obj.Loop);
				obj.m_thread.IsBackground = true;
				obj.m_thread.Name = "zbCombatRecordSave";
				obj.m_thread.Start();
				return obj;
			}
			catch (Exception ex)
			{
				Debug.LogWarning((object)("[zbCombatRecorder] 后台存档线程不可用(主包 AOT 裁剪了线程成员?),回退同步存档: " + ex.Message));
				return null;
			}
		}

		public void EnqueueSave(zbCombatRecordSaveOp op, Action<zbCombatRecordSaveOp> mergeSticky)
		{
			lock (m_gate)
			{
				for (int num = m_ops.Count - 1; num >= 0; num--)
				{
					if (m_ops[num].snapshot != null)
					{
						mergeSticky?.Invoke(m_ops[num]);
						m_ops.RemoveAt(num);
					}
				}
				m_ops.Add(op);
			}
		}

		public void EnqueueClear(zbCombatRecordSaveOp op)
		{
			lock (m_gate)
			{
				for (int num = m_ops.Count - 1; num >= 0; num--)
				{
					if (m_ops[num].snapshot != null)
					{
						m_ops.RemoveAt(num);
					}
				}
				m_ops.Add(op);
			}
		}

		public void RequestStopAndDrain(int timeoutMs)
		{
			lock (m_gate)
			{
				m_stopping = true;
			}
			for (int i = 0; i < timeoutMs; i += 10)
			{
				bool flag;
				lock (m_gate)
				{
					flag = m_ops.Count == 0 && !m_executing;
				}
				if (flag)
				{
					return;
				}
				Thread.Sleep(10);
			}
			Debug.LogError((object)("[zbCombatRecorder] 后台存档线程 " + timeoutMs + "ms 内未完成排干,放弃剩余存档操作"));
		}

		private void Loop()
		{
			while (true)
			{
				zbCombatRecordSaveOp obj = null;
				lock (m_gate)
				{
					if (m_ops.Count > 0)
					{
						obj = m_ops[0];
						m_ops.RemoveAt(0);
						m_executing = true;
						goto IL_006b;
					}
					if (m_stopping)
					{
						break;
					}
					Monitor.Wait(m_gate, 250);
				}
				continue;
				IL_006b:
				try
				{
					m_execute(obj);
				}
				finally
				{
					lock (m_gate)
					{
						m_executing = false;
					}
				}
			}
		}

		private static void ProbeThreadingCapabilities()
		{
			object obj = new object();
			lock (obj)
			{
				Monitor.Wait(obj, 0);
			}
			Thread thread = new Thread(ProbeThreadBody);
			thread.IsBackground = true;
			thread.Name = "zbSaveProbe";
			thread.Start();
			Thread.Sleep(0);
		}

		private static void ProbeThreadBody()
		{
		}
	}

	private float lastSaveTime;

	private float nextSaveTime;

	private float SaveTimeRate = 10f;

	private string m_strFormatVersion;

	private string m_strFilePathVersion;

	private string m_strFilePathData;

	public stCombatRecorderData loadedData;

	public zbSafeInt curRound = new zbSafeInt(0, "zbCombatRecorder.curRound");

	private List<stRecordMonsterSpawnState> curRoundMonsterSpawn = new List<stRecordMonsterSpawnState>();

	private zbCombatRecordSaveWorker m_saveWorker;

	private static bool s_saveWorkerUnavailable;

	public bool needSelectMod;

	public int selectNum;

	public int selectMax;

	public int curRefreshNum;

	public List<int> selectMod = new List<int>();

	private stCurrModWheel currModWheel = new stCurrModWheel();

	private stCurrFightForwardWnd currFightForward = new stCurrFightForwardWnd();

	public void Init()
	{
		m_strFormatVersion = "0.75";
		m_strFilePathVersion = Path.Combine(Application.persistentDataPath, "combatRecorder_version.txt");
		m_strFilePathData = Path.Combine(Application.persistentDataPath, "combatRecorder_data.txt");
		lastSaveTime = 0f;
		nextSaveTime = lastSaveTime + SaveTimeRate;
		ResetAsyncSaveState();
	}

	public void Free()
	{
		FlushPendingSaves();
	}

	public void ClearRecord()
	{
		EnqueueClearRecord();
	}

	public void SetMonsterSpawn(int _curRound, List<stRecordMonsterSpawnState> _curRoundMonsterSpawn)
	{
		curRound.Value = _curRound;
		curRoundMonsterSpawn = _curRoundMonsterSpawn;
		if (zbCombatSystem.Instance.GetBattleType() == EBattleFuncType.MainQuest && zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId == 1)
		{
			int progressTrack = 301 + (int)curRound;
			zbProgressEventHelper.Instance.SetProgressTrack(progressTrack);
		}
	}

	public void Update()
	{
		lastSaveTime += Time.deltaTime;
		if (lastSaveTime > nextSaveTime)
		{
			Save();
			nextSaveTime = lastSaveTime + SaveTimeRate;
		}
	}

	private void EnqueueSave(stCombatRecorderData snapshot)
	{
		if (snapshot == null)
		{
			return;
		}
		zbCombatRecordSaveWorker zbCombatRecordSaveWorker2 = EnsureSaveWorker();
		if (zbCombatRecordSaveWorker2 != null)
		{
			zbCombatRecordSaveWorker2.EnqueueSave(new zbCombatRecordSaveOp
			{
				snapshot = snapshot
			}, delegate(zbCombatRecordSaveOp oldOp)
			{
				MergeStickyFlags(oldOp.snapshot, snapshot);
			});
		}
		else
		{
			ExecuteSaveOp(new zbCombatRecordSaveOp
			{
				snapshot = snapshot
			});
		}
	}

	private void EnqueueClearRecord()
	{
		zbCombatRecordSaveWorker zbCombatRecordSaveWorker2 = EnsureSaveWorker();
		if (zbCombatRecordSaveWorker2 != null)
		{
			zbCombatRecordSaveWorker2.EnqueueClear(new zbCombatRecordSaveOp());
		}
		else
		{
			ExecuteSaveOp(new zbCombatRecordSaveOp());
		}
	}

	private void FlushPendingSaves()
	{
		zbCombatRecordSaveWorker saveWorker = m_saveWorker;
		if (saveWorker != null)
		{
			m_saveWorker = null;
			try
			{
				saveWorker.RequestStopAndDrain(3000);
			}
			catch (Exception ex)
			{
				Debug.LogWarning((object)("[zbCombatRecorder] 排干后台存档线程异常(已忽略): " + ex.Message));
			}
		}
	}

	private void ResetAsyncSaveState()
	{
		FlushPendingSaves();
	}

	private zbCombatRecordSaveWorker EnsureSaveWorker()
	{
		if (s_saveWorkerUnavailable)
		{
			return null;
		}
		if (m_saveWorker == null)
		{
			zbCombatRecordSaveWorker zbCombatRecordSaveWorker2 = null;
			try
			{
				zbCombatRecordSaveWorker2 = zbCombatRecordSaveWorker.TryCreate(ExecuteSaveOp);
			}
			catch (Exception)
			{
				zbCombatRecordSaveWorker2 = null;
			}
			m_saveWorker = zbCombatRecordSaveWorker2;
			if (zbCombatRecordSaveWorker2 == null)
			{
				s_saveWorkerUnavailable = true;
			}
		}
		return m_saveWorker;
	}

	private void ExecuteSaveOp(zbCombatRecordSaveOp op)
	{
		try
		{
			if (op.snapshot == null)
			{
				SaveToFile(m_strFilePathVersion, "");
				return;
			}
			BeginSave();
			string strContent = JsonConvert.SerializeObject((object)op.snapshot);
			SaveToFile(m_strFilePathData, strContent);
			EndSave();
		}
		catch (Exception)
		{
		}
	}

	private static void MergeStickyFlags(stCombatRecorderData oldData, stCombatRecorderData newData)
	{
		newData.curSelectMod.needSelectMod |= oldData.curSelectMod.needSelectMod;
		newData.curModWheel.needModWheel |= oldData.curModWheel.needModWheel;
		newData.currFightForward.needFightForward |= oldData.currFightForward.needFightForward;
	}

	private static List<stRecordMonsterSpawnState> CopyMonsterSpawnStateList(List<stRecordMonsterSpawnState> source)
	{
		if (source == null)
		{
			return null;
		}
		List<stRecordMonsterSpawnState> list = new List<stRecordMonsterSpawnState>(source.Count);
		for (int i = 0; i < source.Count; i++)
		{
			stRecordMonsterSpawnState stRecordMonsterSpawnState2 = source[i];
			list.Add(new stRecordMonsterSpawnState
			{
				round = stRecordMonsterSpawnState2.round,
				tid = stRecordMonsterSpawnState2.tid,
				spawned = stRecordMonsterSpawnState2.spawned,
				turntable = stRecordMonsterSpawnState2.turntable
			});
		}
		return list;
	}

	public bool Load()
	{
		string text = LoadFromFile(m_strFilePathVersion);
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		if (text != m_strFormatVersion)
		{
			return false;
		}
		string text2 = LoadFromFile(m_strFilePathData);
		if (!string.IsNullOrEmpty(text2))
		{
			try
			{
				loadedData = JsonConvert.DeserializeObject<stCombatRecorderData>(text2);
			}
			catch (Exception)
			{
				return false;
			}
		}
		else
		{
			loadedData = new stCombatRecorderData();
		}
		if (loadedData.roleId != zbSelf.Instance.GetRoleId())
		{
			return false;
		}
		if (loadedData.recordCombatSystem.battleInfo == null)
		{
			return false;
		}
		if (zbSelf.Instance.timeHandler.GetServerTime() >= loadedData.recordCombatSystem.battleInfo.BattleBaseInfo.EndTm)
		{
			return false;
		}
		return true;
	}

	public void Restore()
	{
		if (loadedData != null && loadedData.recordCombatSystem.battleInfo != null)
		{
			zbSelf.Instance.combatHandler.StartShowCombat(loadedData.recordCombatSystem.battleInfo, loadedData.recordCombatSystem.paramId, this);
		}
	}

	public string LoadFromFile(string strFilePath)
	{
		try
		{
			if (!File.Exists(strFilePath))
			{
				return string.Empty;
			}
			byte[] bytes = zbFileEncrypUtil.DecryptFile(strFilePath);
			return Encoding.UTF8.GetString(bytes);
		}
		catch (Exception)
		{
			return string.Empty;
		}
	}

	public void NeedModSelectSave(int _selectNum, int _selectMax, int _curRefreshNum, List<int> _selectMod)
	{
		needSelectMod = true;
		selectNum = _selectNum;
		selectMax = _selectMax;
		curRefreshNum = _curRefreshNum;
		selectMod = _selectMod;
		Save();
	}

	public void NeedModWheelSave(int _monsterTid, int[] _selectMod)
	{
		currModWheel.needModWheel = true;
		currModWheel.monsterTid = _monsterTid;
		currModWheel.selectMod = _selectMod.ToArray();
		Save();
	}

	public void NeedFightForwardSave(int _selectNum, int _selectMax, int _curRefreshNum, List<int> _selectMod)
	{
		currFightForward.needFightForward = true;
		currFightForward.selectNum = _selectNum;
		currFightForward.selectMax = _selectMax;
		currFightForward.curRefreshNum = _curRefreshNum;
		currFightForward.selectMod = _selectMod;
		Save();
	}

	public void Save()
	{
		//IL_0624: Unknown result type (might be due to invalid IL or missing references)
		//IL_0646: Unknown result type (might be due to invalid IL or missing references)
		//IL_0668: Unknown result type (might be due to invalid IL or missing references)
		if (zbCombatSystem.Instance.dataResult != null || zbCombatSystem.Instance.isPVPBattleType)
		{
			return;
		}
		stCombatRecorderData stCombatRecorderData2 = new stCombatRecorderData();
		stCombatRecorderData2.roleId = zbSelf.Instance.GetRoleId();
		stCombatRecorderData2.recordCombatSystem.battleInfo = zbCombatSystem.Instance.battleInfo;
		stCombatRecorderData2.recordCombatSystem.paramId = zbCombatSystem.Instance.paramId;
		stCombatRecorderData2.recordCombatSystem.accumTime = zbCombatSystem.Instance.accumTime;
		stCombatRecorderData2.recordCombatSystem.countDown = zbCombatSystem.Instance.countDown;
		stCombatRecorderData2.recordCombatSystem.startServerTime = zbCombatSystem.Instance.startServerTime;
		stCombatRecorderData2.recordCombatSystem.refreshSelectNum = zbCombatSystem.Instance.refreshSelectNum;
		stCombatRecorderData2.recordCombatSystem.refreshWheelNum = zbCombatSystem.Instance.refreshWheelNum;
		stCombatRecorderData2.recordCombatSystem.fightRebornNum = zbCombatSystem.Instance.fightRebornNum;
		stCombatRecorderData2.recordCombatSystem.lastRoundFlag = zbCombatSystem.Instance.lastRoundFlag;
		stCombatRecorderData2.recordCombatSystem.lastRoundTime = zbCombatSystem.Instance.lastRoundTime;
		stCombatRecorderData2.recordCombatSystem.scrollTotalDistance = zbCombatSystem.Instance.scrollTotalDistance;
		stCombatRecorderData2.recordCombatSystem.scrollDistance = zbCombatSystem.Instance.scrollDistance;
		stCombatRecorderData2.recordCombatSystem.celebrateDistance = zbCombatSystem.Instance.celebrateDistance;
		stCombatRecorderData2.recordCombatSystem.endlessHordeStartLevel = zbCombatSystem.Instance.endlessHordeStartLevel;
		stCombatRecorderData2.recordCombatSystem.celebratePreTime = zbCombatSystem.Instance.celebratePreTime;
		stCombatRecorderData2.monster3006Reborn.SetMonster3006Reborn(zbCombatSystem.Instance.monster3006Reborn);
		stCombatRecorderData2.recordCombatSystem.sceneId = zbCombatSystem.Instance.combatData.combatScene.sceneId;
		zbMonster closestBossMonster = zbCombatSystem.Instance.combatData.monsters.GetClosestBossMonster(15f, ignoreUnSelect: true);
		if ((Object)(object)closestBossMonster != (Object)null && closestBossMonster.needRestoreData)
		{
			closestBossMonster.monsterSkillIndexClass.Save(stCombatRecorderData2.monsterBoss);
		}
		stCombatRecorderData2.recordDefense.maxHp = zbCombatSystem.Instance.combatData.defense.maxHp;
		stCombatRecorderData2.recordDefense.curHp = zbCombatSystem.Instance.combatData.defense.curHp;
		stCombatRecorderData2.recordDefense.shieldHp = zbCombatSystem.Instance.combatData.defense.shieldHp;
		stCombatRecorderData2.recordDefense.damageImmunity = zbCombatSystem.Instance.combatData.defense.damageImmunity;
		stCombatRecorderData2.recordDefense.damageDecr = zbCombatSystem.Instance.combatData.defense.damageDecr;
		foreach (KeyValuePair<int, zbWallSkillHandleBase> item3 in zbCombatSystem.Instance.combatData.defenseMgr.defenseListP1)
		{
			List<int> list = new List<int>();
			int[] saveRecord = item3.Value.wallSkillDataBase.saveRecord;
			foreach (int item in saveRecord)
			{
				list.Add(item);
			}
			stCombatRecorderData2.recordDefense.wallSkillIndexsP1[item3.Key] = list;
		}
		foreach (KeyValuePair<int, zbWallSkillHandleBase> item4 in zbCombatSystem.Instance.combatData.defenseMgr.defenseListP2)
		{
			List<int> list2 = new List<int>();
			int[] saveRecord = item4.Value.wallSkillDataBase.saveRecord;
			foreach (int item2 in saveRecord)
			{
				list2.Add(item2);
			}
			stCombatRecorderData2.recordDefense.wallSkillIndexsP2[item4.Key] = list2;
		}
		stCombatRecorderData2.recordExp.monsterKilledExp = zbCombatSystem.Instance.combatData.combatExp.monsterKilledExp;
		stCombatRecorderData2.recordExp.monsterKilledCount = zbCombatSystem.Instance.combatData.combatExp.monsterKilledCount;
		foreach (zbModBase mod in zbCombatSystem.Instance.combatData.masterTeam.modMgr.modList)
		{
			if (mod.isUserSelect)
			{
				stRecordUseSelectMod stRecordUseSelectMod2 = new stRecordUseSelectMod();
				stRecordUseSelectMod2.modId = mod.modRecord.id;
				stRecordUseSelectMod2.level = mod.level;
				stCombatRecorderData2.userSelectModList.Add(stRecordUseSelectMod2);
			}
		}
		foreach (KeyValuePair<int, long> heroDamageStat in zbCombatSystem.Instance.combatData.masterTeam.combatStat.heroDamageStats)
		{
			stRecordHeroDamageStat stRecordHeroDamageStat2 = new stRecordHeroDamageStat();
			stRecordHeroDamageStat2.heroTid = heroDamageStat.Key;
			stRecordHeroDamageStat2.damage = heroDamageStat.Value;
			stCombatRecorderData2.heroDamageStats.Add(stRecordHeroDamageStat2);
		}
		stCombatRecorderData2.damageBossSum = zbCombatSystem.Instance.combatData.masterTeam.combatStat.damageBossSum;
		stCombatRecorderData2.recordMonsterSpawn.round = curRound;
		stCombatRecorderData2.recordMonsterSpawn.monsterSpawnState = CopyMonsterSpawnStateList(curRoundMonsterSpawn);
		foreach (KeyValuePair<int, zbMonster> monster in zbCombatSystem.Instance.combatData.monsters.monsterList)
		{
			if ((long)monster.Value.curHp > 0)
			{
				stRecordMonster stRecordMonster2 = new stRecordMonster();
				stRecordMonster2.tid = monster.Value.monsterRecord.id;
				stRecordMonster2.curHp = monster.Value.curHp;
				stRecordMonster2.curExp = monster.Value.curExp;
				stRecordMonster2.shieldHp = monster.Value.shieldHp;
				stRecordMonster2.maxShieldHp = monster.Value.maxShieldHp;
				stRecordMonster2.position = new stVector();
				stRecordMonster2.position.x = ((Component)monster.Value).transform.localPosition.x;
				stRecordMonster2.position.y = ((Component)monster.Value).transform.localPosition.y;
				stRecordMonster2.position.z = ((Component)monster.Value).transform.localPosition.z;
				stCombatRecorderData2.monsterList.Add(stRecordMonster2);
			}
		}
		foreach (zbHero hero in zbCombatSystem.Instance.combatData.masterTeam.heroList)
		{
			stRecordHero stRecordHero2 = new stRecordHero();
			stRecordHero2.heroTid = hero.GetHeroTid();
			stRecordHero2.nextFireTime = hero.nextFireTime;
			stRecordHero2.needDurationProgress = hero.needDurationProgress;
			stRecordHero2.needDurationProgressMax = hero.needDurationProgressMax;
			stCombatRecorderData2.heroList.Add(stRecordHero2);
			if (hero.GetSkillType() == 1)
			{
				zbHeroQiang zbHeroQiang2 = (zbHeroQiang)hero;
				if ((Object)(object)zbHeroQiang2 != (Object)null)
				{
					stCombatRecorderData2.heroQiang = new stRecordHeroQiang();
					stCombatRecorderData2.heroQiang.maxAmmo = zbHeroQiang2.maxAmmo;
					stCombatRecorderData2.heroQiang.currentAmmo = zbHeroQiang2.currentAmmo;
					stCombatRecorderData2.heroQiang.reloadTime = zbHeroQiang2.reloadTime;
					stCombatRecorderData2.heroQiang.isReloading = zbHeroQiang2.isReloading;
					stCombatRecorderData2.heroQiang.reloadedTime = zbHeroQiang2.reloadedTime;
					stCombatRecorderData2.heroQiang.progress = zbHeroQiang2.progress;
					stCombatRecorderData2.heroQiang.hitNumber = zbHeroQiang2.hitNumber;
					stCombatRecorderData2.heroQiang.limitlessTime = zbHeroQiang2.limitlessTime;
					break;
				}
			}
		}
		stCombatRecorderData2.teamCommom = new stRecordTeamComm();
		stCombatRecorderData2.teamCommom.mechaMaxCount = zbCombatSystem.Instance.combatData.masterTeam.mechaMaxCount;
		stCombatRecorderData2.teamCommom.mechaCallCount = zbCombatSystem.Instance.combatData.masterTeam.mechaCallCount;
		if (zbCombatSystem.Instance.combatData.helperTeam != null)
		{
			foreach (zbModBase mod2 in zbCombatSystem.Instance.combatData.helperTeam.modMgr.modList)
			{
				if (mod2.isUserSelect)
				{
					stRecordUseSelectMod stRecordUseSelectMod3 = new stRecordUseSelectMod();
					stRecordUseSelectMod3.modId = mod2.modRecord.id;
					stRecordUseSelectMod3.level = mod2.level;
					stCombatRecorderData2.userHelpSelectModList.Add(stRecordUseSelectMod3);
				}
			}
			foreach (KeyValuePair<int, long> heroDamageStat2 in zbCombatSystem.Instance.combatData.helperTeam.combatStat.heroDamageStats)
			{
				stRecordHeroDamageStat stRecordHeroDamageStat3 = new stRecordHeroDamageStat();
				stRecordHeroDamageStat3.heroTid = heroDamageStat2.Key;
				stRecordHeroDamageStat3.damage = heroDamageStat2.Value;
				stCombatRecorderData2.heroHelpDamageStats.Add(stRecordHeroDamageStat3);
			}
			foreach (zbHero hero2 in zbCombatSystem.Instance.combatData.helperTeam.heroList)
			{
				stRecordHero stRecordHero3 = new stRecordHero();
				stRecordHero3.heroTid = hero2.GetHeroTid();
				stRecordHero3.nextFireTime = hero2.nextFireTime;
				stRecordHero3.needDurationProgress = hero2.needDurationProgress;
				stRecordHero3.needDurationProgressMax = hero2.needDurationProgressMax;
				stCombatRecorderData2.heroHelpList.Add(stRecordHero3);
				if (hero2.GetSkillType() == 1)
				{
					zbHeroQiang zbHeroQiang3 = (zbHeroQiang)hero2;
					if ((Object)(object)zbHeroQiang3 != (Object)null)
					{
						stCombatRecorderData2.heroHelpQiang = new stRecordHeroQiang();
						stCombatRecorderData2.heroHelpQiang.maxAmmo = zbHeroQiang3.maxAmmo;
						stCombatRecorderData2.heroHelpQiang.currentAmmo = zbHeroQiang3.currentAmmo;
						stCombatRecorderData2.heroHelpQiang.reloadTime = zbHeroQiang3.reloadTime;
						stCombatRecorderData2.heroHelpQiang.isReloading = zbHeroQiang3.isReloading;
						stCombatRecorderData2.heroHelpQiang.reloadedTime = zbHeroQiang3.reloadedTime;
						stCombatRecorderData2.heroHelpQiang.progress = zbHeroQiang3.progress;
						stCombatRecorderData2.heroHelpQiang.hitNumber = zbHeroQiang3.hitNumber;
						stCombatRecorderData2.heroHelpQiang.limitlessTime = zbHeroQiang3.limitlessTime;
						break;
					}
				}
			}
			stCombatRecorderData2.teamHelpCommom = new stRecordTeamComm();
			stCombatRecorderData2.teamHelpCommom.mechaMaxCount = zbCombatSystem.Instance.combatData.helperTeam.mechaMaxCount;
			stCombatRecorderData2.teamHelpCommom.mechaCallCount = zbCombatSystem.Instance.combatData.helperTeam.mechaCallCount;
		}
		stCombatRecorderData2.curSelectMod.needSelectMod = needSelectMod;
		stCombatRecorderData2.curSelectMod.selectNum = selectNum;
		stCombatRecorderData2.curSelectMod.selectMax = selectMax;
		stCombatRecorderData2.curSelectMod.selectMod = new List<int>(selectMod);
		needSelectMod = false;
		stCombatRecorderData2.curModWheel.needModWheel = currModWheel.needModWheel;
		stCombatRecorderData2.curModWheel.monsterTid = currModWheel.monsterTid;
		stCombatRecorderData2.curModWheel.selectMod = currModWheel.selectMod.ToArray();
		currModWheel.needModWheel = false;
		stCombatRecorderData2.currFightForward.needFightForward = currFightForward.needFightForward;
		stCombatRecorderData2.currFightForward.selectNum = currFightForward.selectNum;
		stCombatRecorderData2.currFightForward.selectMax = currFightForward.selectMax;
		stCombatRecorderData2.currFightForward.curRefreshNum = currFightForward.curRefreshNum;
		stCombatRecorderData2.currFightForward.selectMod = new List<int>(currFightForward.selectMod);
		currFightForward.needFightForward = false;
		EBattleFuncType battleType = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.BattleType;
		if (zbSelf.Instance.elementTrialHandler.IsElementLairCombat(battleType))
		{
			stCombatRecorderData2.elementTrial.m_attackType = zbSelf.Instance.elementTrialHandler.m_attackType;
			stCombatRecorderData2.elementTrial.m_levelId = zbSelf.Instance.elementTrialHandler.m_levelId;
		}
		if (battleType == EBattleFuncType.WorldLairHelp || battleType == EBattleFuncType.WorldLair)
		{
			stCombatRecorderData2.worldLair.worldLairLevel = zbSelf.Instance.worldLairHandler.worldLairLevel;
			if (zbCombatSystem.Instance.battleInfo.BattleBaseInfo.BattleType == EBattleFuncType.WorldLairHelp)
			{
				stCombatRecorderData2.worldLair.worldLairUuid = zbSelf.Instance.worldLairHandler.worldLairUuid;
			}
		}
		if (battleType == EBattleFuncType.EliteQuest)
		{
			stCombatRecorderData2.eliteQuest.count = zbSelf.Instance.eliteQuestHandler.fightCount;
			stCombatRecorderData2.eliteQuest.helpId = zbSelf.Instance.eliteQuestHandler.playerId;
		}
		if (battleType == EBattleFuncType.Legendary)
		{
			stCombatRecorderData2.legendary.helpId = zbSelf.Instance.legendaryHandler.playerId;
		}
		if (battleType == EBattleFuncType.WorldBoss)
		{
			if (zbSelf.Instance.worldBossHandler.worldBossInfo != null)
			{
				stCombatRecorderData2.worldBoss.worldBossuuid = zbSelf.Instance.worldBossHandler.worldBossInfo.UUID;
			}
			else
			{
				stCombatRecorderData2.worldBoss.worldBossuuid = zbSelf.Instance.worldBossHandler.worldBossuuid;
			}
		}
		if (battleType == EBattleFuncType.Arena)
		{
			stCombatRecorderData2.arena.playerId = zbSelf.Instance.arenaHandler.playerId;
			stCombatRecorderData2.arena.rankIndex = zbSelf.Instance.arenaHandler.rankIndex;
		}
		if (battleType == EBattleFuncType.GuardVerify)
		{
			stCombatRecorderData2.guardVerify.CombatIndex = zbSelf.Instance.guardHandler.CombatIndex;
		}
		if (battleType == EBattleFuncType.CrossLadderMatch || battleType == EBattleFuncType.CrossLadderUpgrade)
		{
			stCombatRecorderData2.crossLadder.playerId = zbSelf.Instance.crossLadderHandler.playerId;
		}
		if (battleType == EBattleFuncType.MineWar || battleType == EBattleFuncType.CrossMine)
		{
			stCombatRecorderData2.mineWar.currPointId = zbSelf.Instance.mineWarHandler.currPointId;
			stCombatRecorderData2.mineWar.currPlayerId = zbSelf.Instance.mineWarHandler.currPlayerId;
			stCombatRecorderData2.mineWar.currServerId = zbSelf.Instance.mineWarHandler.currServerId;
		}
		if (battleType == EBattleFuncType.PeakArena)
		{
			stCombatRecorderData2.peakArena.currRank = zbSelf.Instance.crossPeakHandler.currRank;
			stCombatRecorderData2.peakArena.currPlayerId = zbSelf.Instance.crossPeakHandler.currPlayerId;
		}
		if (battleType == EBattleFuncType.Truck || battleType == EBattleFuncType.Train)
		{
			stCombatRecorderData2.truck.currTransportId = zbSelf.Instance.truckHandler.currTransportId;
			stCombatRecorderData2.truck.currPlayerId = zbSelf.Instance.truckHandler.currPlayerId;
		}
		if (battleType == EBattleFuncType.LegionWar)
		{
			stCombatRecorderData2.legionWar.currPlayerId = zbSelf.Instance.legionWarHandler.currPlayerId;
		}
		if (battleType == EBattleFuncType.BossSubject)
		{
			stCombatRecorderData2.bossSubject.currUUID = zbSelf.Instance.bossSubjectHandler.currUUID;
			stCombatRecorderData2.bossSubject.currBossLevel = zbSelf.Instance.bossSubjectHandler.currBossLevel;
			stCombatRecorderData2.bossSubject.currDamageAdd = zbSelf.Instance.bossSubjectHandler.currDamageAdd;
		}
		if (battleType == EBattleFuncType.WallSubject)
		{
			stCombatRecorderData2.wallSubject.currUUID = zbSelf.Instance.wallSubjectHandler.currUUID;
			stCombatRecorderData2.wallSubject.currLevelId = zbSelf.Instance.wallSubjectHandler.currLevelId;
		}
		if (battleType == EBattleFuncType.MechaSubject)
		{
			stCombatRecorderData2.mechaSubject.currUUID = zbSelf.Instance.mechaSubjectHandler.currUUID;
			stCombatRecorderData2.mechaSubject.currLevelId = zbSelf.Instance.mechaSubjectHandler.currLevelId;
		}
		EnqueueSave(stCombatRecorderData2);
	}

	public void BeginSave()
	{
		SaveToFile(m_strFilePathVersion, "");
	}

	public void EndSave()
	{
		SaveToFile(m_strFilePathVersion, m_strFormatVersion);
	}

	public void SaveToFile(string strFilePath, string strContent)
	{
		try
		{
			if (string.IsNullOrEmpty(strContent))
			{
				File.WriteAllText(strFilePath, "");
				return;
			}
			byte[] bytes = zbFileEncrypUtil.EncryptData(Encoding.UTF8.GetBytes(strContent));
			File.WriteAllBytes(strFilePath, bytes);
		}
		catch (Exception)
		{
		}
	}
}
