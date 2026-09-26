using System;
using System.Collections;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMonsterSpawner : MonoBehaviour
{
	private zbCombatData m_combatData;

	private Vector2 m_spawnAreaMin;

	private Vector2 m_spawnAreaMax;

	[NonSerialized]
	public bool monsterSpawnerFinish;

	private Coroutine m_coroutine;

	public void InitData(zbCombatData combatData)
	{
		m_combatData = combatData;
	}

	private void Start()
	{
		RefreshArea();
		SpawnWaves();
	}

	public void RefreshArea(float offsetY = 0f)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		if (zbCombatSystem.Instance.isPVPBattleType)
		{
			((Component)this).gameObject.transform.position = new Vector3(((Component)this).gameObject.transform.position.x, 2.0999994f + offsetY, ((Component)this).gameObject.transform.position.z);
		}
		else
		{
			((Component)this).gameObject.transform.position = new Vector3(((Component)this).gameObject.transform.position.x, 7.4f, ((Component)this).gameObject.transform.position.z);
		}
		m_spawnAreaMin.x = ((Component)this).gameObject.transform.position.x - ((Component)this).gameObject.transform.localScale.x / 2f;
		m_spawnAreaMin.y = ((Component)this).gameObject.transform.position.y - ((Component)this).gameObject.transform.localScale.y / 2f;
		m_spawnAreaMax.x = ((Component)this).gameObject.transform.position.x + ((Component)this).gameObject.transform.localScale.x / 2f;
		m_spawnAreaMax.y = ((Component)this).gameObject.transform.position.y + ((Component)this).gameObject.transform.localScale.y / 2f;
	}

	public void SpawnWaves()
	{
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		zbCombatSystem.Instance.lastRoundFlag = false;
		if (m_coroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_coroutine);
			m_coroutine = null;
		}
		int curRound = 0;
		float accumTime = 0f;
		List<stRecordMonsterSpawnState> recordMonsterSpawnState = null;
		if (zbCombatSystem.Instance.restoreData != null)
		{
			curRound = zbCombatSystem.Instance.restoreData.loadedData.recordMonsterSpawn.round;
			accumTime = zbCombatSystem.Instance.restoreData.loadedData.recordCombatSystem.accumTime;
			recordMonsterSpawnState = zbCombatSystem.Instance.restoreData.loadedData.recordMonsterSpawn.monsterSpawnState;
			foreach (stRecordMonster monster in zbCombatSystem.Instance.restoreData.loadedData.monsterList)
			{
				zbMonster zbMonster2 = SpawnMonster(monster.tid, monster.turntable, monster.curExp > 0, _isRestoreData: true, 0.0);
				if (Object.op_Implicit((Object)(object)zbMonster2))
				{
					zbMonster2.SetCurHp(monster.curHp);
					zbMonster2.maxShieldHp = monster.maxShieldHp;
					zbMonster2.SetShieldHp(monster.shieldHp);
					zbMonster2.curExp = monster.curExp;
					((Component)zbMonster2).transform.localPosition = new Vector3(monster.position.x, monster.position.y, monster.position.z);
					if (zbCombatSystem.Instance.restoreData.loadedData.monsterBoss.monsterTid == zbMonster2.monsterRecord.id)
					{
						zbMonster2.monsterSkillIndexClass.RestoreData(zbCombatSystem.Instance.restoreData.loadedData.monsterBoss);
					}
				}
			}
		}
		if (zbCombatSystem.Instance.GetDefenseType() != EDefenseType.CarDefense)
		{
			m_coroutine = ((MonoBehaviour)this).StartCoroutine(SpawnWave(curRound, accumTime, recordMonsterSpawnState));
		}
		else
		{
			m_coroutine = ((MonoBehaviour)this).StartCoroutine(SpawnWaveCarDefense(curRound, accumTime, recordMonsterSpawnState));
		}
		recordMonsterSpawnState = null;
	}

	private IEnumerator SpawnWave(int curRound, float accumTime, List<stRecordMonsterSpawnState> recordMonsterSpawnState)
	{
		return SpawnWaveCore(curRound, accumTime, recordMonsterSpawnState, null, null);
	}

	private IEnumerator SpawnWaveCarDefense(int curRound, float accumTime, List<stRecordMonsterSpawnState> recordMonsterSpawnState)
	{
		return SpawnWaveCore(curRound, accumTime, recordMonsterSpawnState, (BattleMonsterRoundDetail roundDetail) => zbCombatSystem.Instance.scrollDistance < (float)(roundDetail.BornStart * zbCombatSystem.Instance.carMaxSpeed), (BattleMonsterRoundDetail roundDetail, float interval) => (!zbCombatSystem.Instance.m_isCelebrating) ? interval : (interval / 2f));
	}

	private List<stRecordMonsterSpawnState> GenerateMonsterList(BattleMonsterRoundDetail roundDetail)
	{
		List<stRecordMonsterSpawnState> list = new List<stRecordMonsterSpawnState>();
		List<stRecordMonsterSpawnState> list2 = new List<stRecordMonsterSpawnState>();
		foreach (BattleBornMonsterDetail born in roundDetail.BornList)
		{
			for (int i = 0; i < born.Num; i++)
			{
				stRecordMonsterSpawnState stRecordMonsterSpawnState2 = new stRecordMonsterSpawnState();
				stRecordMonsterSpawnState2.round = roundDetail.Round;
				stRecordMonsterSpawnState2.tid = born.MonsterId;
				stRecordMonsterSpawnState2.spawned = false;
				stRecordMonsterSpawnState2.turntable = (int)born.Turntable;
				if (zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(born.MonsterId).type != 1)
				{
					list.Add(stRecordMonsterSpawnState2);
				}
				else
				{
					list2.Add(stRecordMonsterSpawnState2);
				}
			}
		}
		zbUtil.Shuffle(list2);
		foreach (stRecordMonsterSpawnState item in list)
		{
			list2.Insert(0, item);
		}
		return list2;
	}

	private IEnumerator SpawnWaveCore(int curRound, float accumTime, List<stRecordMonsterSpawnState> recordMonsterSpawnState, Func<BattleMonsterRoundDetail, bool> waitCondition, Func<BattleMonsterRoundDetail, float, float> intervalAdjuster)
	{
		yield return null;
		while (zbCombatSystem.Instance != null && zbCombatSystem.Instance.IsMonsterPreHeatPending)
		{
			yield return null;
		}
		int round = curRound;
		while (round < zbCombatSystem.Instance.battleInfo.MonsterInfo.RoundList.Count)
		{
			BattleMonsterRoundDetail roundDetail = zbCombatSystem.Instance.battleInfo.MonsterInfo.RoundList[round];
			while (waitCondition != null && waitCondition(roundDetail))
			{
				yield return null;
			}
			float num = roundDetail.BornEnd - roundDetail.BornStart;
			float interval = num / (float)roundDetail.BornCount;
			if (intervalAdjuster != null)
			{
				interval = intervalAdjuster(roundDetail, interval);
			}
			List<stRecordMonsterSpawnState> monsterList = ((round != curRound || recordMonsterSpawnState == null) ? GenerateMonsterList(roundDetail) : new List<stRecordMonsterSpawnState>(recordMonsterSpawnState));
			zbCombatSystem.Instance.combatRecorder.SetMonsterSpawn(round, monsterList);
			float timer = 0f;
			int spawnedCount = 0;
			int spawnedThisFrame = 0;
			for (int i = 0; i < monsterList.Count; i++)
			{
				if (monsterList[i].spawned)
				{
					spawnedCount++;
				}
			}
			double monsterDamageLayer = Math.Pow(zbSelf.Instance.pvpHandler.monsterDamageLayer, round - 1);
			while (spawnedCount < monsterList.Count)
			{
				timer += Time.deltaTime;
				while (timer >= interval && spawnedCount < monsterList.Count && spawnedThisFrame < 6)
				{
					timer -= interval;
					stRecordMonsterSpawnState stRecordMonsterSpawnState2 = monsterList[spawnedCount];
					if (!stRecordMonsterSpawnState2.spawned)
					{
						stRecordMonsterSpawnState2.spawned = true;
						SpawnMonster(stRecordMonsterSpawnState2.tid, stRecordMonsterSpawnState2.turntable, giveExp: true, _isRestoreData: false, monsterDamageLayer);
						monsterList[spawnedCount] = stRecordMonsterSpawnState2;
					}
					spawnedCount++;
					spawnedThisFrame++;
				}
				spawnedThisFrame = 0;
				yield return null;
			}
			if (zbCombatSystem.Instance.battleInfo.MonsterInfo.RoundList.Count > round + 1)
			{
				BattleMonsterRoundDetail battleMonsterRoundDetail = zbCombatSystem.Instance.battleInfo.MonsterInfo.RoundList[round + 1];
				float waitTime = battleMonsterRoundDetail.BornStart - roundDetail.BornEnd;
				if (waitTime > 0f)
				{
					float waitTimer = 0f;
					while (waitTimer < waitTime)
					{
						waitTimer += Time.deltaTime;
						yield return null;
					}
				}
			}
			int num2 = round + 1;
			round = num2;
		}
		if (!zbCombatSystem.Instance.lastRoundFlag)
		{
			zbCombatSystem.Instance.lastRoundFlag = true;
			zbCombatSystem.Instance.lastRoundTime = zbCombatSystem.Instance.accumTime;
		}
		monsterSpawnerFinish = true;
		zbCombatAtlasMerger.Instance.DumpRuntimeCoverage();
	}

	private zbMonster SpawnMonster(int monsterTid, int turntable, bool giveExp, bool _isRestoreData, double monsterDamageLayer)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		zbMonsterCsvRecord recordByKey = zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(monsterTid);
		float num = Rand.Range(m_spawnAreaMin.x, m_spawnAreaMax.x);
		float num2 = Rand.Range(m_spawnAreaMin.y, m_spawnAreaMax.y);
		if (recordByKey.type != 1)
		{
			num = 0f;
		}
		Vector3 position = default(Vector3);
		((Vector3)(ref position))._002Ector(num, num2, 0f);
		zbMonster monster = m_combatData.monsterPoolMgr.GetMonster(monsterTid);
		((Component)monster).gameObject.transform.SetParent(((Component)this).transform.parent);
		((Component)monster).gameObject.transform.position = position;
		((Component)monster).gameObject.transform.localScale = Vector3.one;
		if (recordByKey.type != 1 && zbCombatSystem.Instance.GetBattleType() == EBattleFuncType.MainQuest && zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId == 1)
		{
			zbCombatWnd zbCombatWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Combat) as zbCombatWnd;
			zbUIMgr.Instance.ShowGuideFinger((VisualElement)(object)zbCombatWnd2.m_btnFocus);
		}
		if (recordByKey.type == 3)
		{
			((zbCombatWnd)zbUIMgr.Instance.GetWndById(EWndId.Combat)).ActionBossComing();
		}
		monster.Init(m_combatData, monsterTid, turntable, _giveExp: true, _isRestoreData);
		if (zbCombatSystem.Instance.isPVPBattleType)
		{
			monster.PvpAddAttr(monsterDamageLayer + 1.0);
		}
		return monster;
	}

	public float GetMinX()
	{
		return m_spawnAreaMin.x;
	}

	public float GetMaxX()
	{
		return m_spawnAreaMax.x;
	}

	public float GetMinY()
	{
		return m_spawnAreaMin.y;
	}

	public float GetMaxY()
	{
		return m_spawnAreaMax.y;
	}
}
