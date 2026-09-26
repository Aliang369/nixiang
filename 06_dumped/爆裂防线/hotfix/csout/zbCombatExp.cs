using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;

public class zbCombatExp
{
	private zbCombatData combatData;

	private int lastLevel;

	public zbSafeInt monsterKilledExp = new zbSafeInt(0, "zbCombatDefense.monsterKilledExp");

	public zbSafeInt monsterTotalCount = new zbSafeInt(0, "zbCombatDefense.monsterTotalCount");

	public zbSafeInt monsterKilledCount = new zbSafeInt(0, "zbCombatDefense.monsterKilledCount");

	public int initLevel;

	public zbSafeInt curLevel = new zbSafeInt(0, "zbCombatExp.curLevel");

	public int maxLevel;

	public zbSafeInt curLevelExp = new zbSafeInt(0, "zbCombatExp.curLevelExp");

	public int curLevelMaxExp;

	private List<stExp> expList = new List<stExp>();

	private int m_lastCalcKillExp = -1;

	public void Init(zbCombatData _combatData)
	{
		combatData = _combatData;
		maxLevel = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.MaxLevel;
		monsterTotalCount.Value = 0;
		foreach (BattleMonsterRoundDetail round in zbCombatSystem.Instance.battleInfo.MonsterInfo.RoundList)
		{
			monsterTotalCount.Value += round.BornCount;
		}
		initLevel = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.InitLevel;
		curLevel.Value = 0;
		lastLevel = curLevel.Value;
		int count = zbCombatSystem.Instance.battleInfo.MonsterInfo.RoundList.Count;
		for (int i = 0; i < count; i++)
		{
			stExp stExp2 = new stExp();
			stExp2.round = i;
			foreach (BattleBornMonsterDetail born in zbCombatSystem.Instance.battleInfo.MonsterInfo.RoundList[i].BornList)
			{
				zbMonsterCsvRecord recordByKey = zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(born.MonsterId);
				if (recordByKey != null && recordByKey.type == 1)
				{
					stExp2.count += born.Num;
				}
			}
			expList.Add(stExp2);
		}
		UpdateExp();
	}

	public void Update()
	{
		int num = monsterKilledExp;
		if (num != m_lastCalcKillExp)
		{
			m_lastCalcKillExp = num;
			UpdateExp();
		}
		if (lastLevel == curLevel.Value || curLevel.Value + initLevel > maxLevel || zbCombatSystem.Instance.combatStateMachine.GetStateId() != ECombatState.Fight)
		{
			return;
		}
		if (zbCombatSystem.Instance.GetBattleType() != EBattleFuncType.EndlessHorde && combatData.dataIndex == 1 && !zbCombatSystem.Instance.isPVPBattleType)
		{
			zbUIMgr.Instance.ShowSelectMod(combatData.masterTeam, curLevel.Value - lastLevel);
		}
		lastLevel = curLevel.Value;
		foreach (stModIndex1052 item in combatData.masterTeam.modIndex1052)
		{
			zbCombatDefense defense = combatData.defense;
			defense.SetCurHp((long)defense.curHp + (long)(item.extraRatio * (float)(long)defense.maxHp));
		}
		if (combatData.helperTeam == null)
		{
			return;
		}
		foreach (stModIndex1052 item2 in combatData.helperTeam.modIndex1052)
		{
			zbCombatDefense defense2 = combatData.defense;
			defense2.SetCurHp((long)defense2.curHp + (long)(item2.extraRatio * (float)(long)defense2.maxHp));
		}
	}

	public void UpdateExp()
	{
		int num = monsterKilledExp;
		int count = expList.Count;
		for (int i = 0; i < count; i++)
		{
			int count2 = expList[i].count;
			if (num < count2)
			{
				curLevel.Value = i;
				curLevelExp.Value = num;
				curLevelMaxExp = count2;
				return;
			}
			num -= count2;
		}
		curLevel.Value = count;
		curLevelExp.Value = 0;
		curLevelMaxExp = curLevelExp.Value;
	}

	public bool IsKillAll()
	{
		if (zbCombatSystem.Instance.GetBattleType() == EBattleFuncType.EndlessHorde)
		{
			return false;
		}
		if (combatData != null && (Object)(object)combatData.monsterSpawner != (Object)null && combatData.monsters != null && combatData.monsters.monsterList != null)
		{
			if (combatData.monsterSpawner.monsterSpawnerFinish)
			{
				return combatData.monsters.monsterList.Count == 0;
			}
			return false;
		}
		return false;
	}

	public void Restore(zbCombatRecorder restoreData)
	{
		monsterKilledExp.Value = restoreData.loadedData.recordExp.monsterKilledExp;
		monsterKilledCount.Value = restoreData.loadedData.recordExp.monsterKilledCount;
		UpdateExp();
		m_lastCalcKillExp = monsterKilledExp;
		lastLevel = curLevel.Value;
	}
}
