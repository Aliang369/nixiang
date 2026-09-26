using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbCombatMonster
{
	public zbCombatData combatData;

	public zbCombatMonsterGrid monsterGrid;

	public Dictionary<int, zbMonster> monsterList = new Dictionary<int, zbMonster>();

	private List<zbMonster> ActiveList = new List<zbMonster>();

	private int selectableCount;

	public void Init(zbCombatData _combatData)
	{
		combatData = _combatData;
		monsterGrid = new zbCombatMonsterGrid();
		selectableCount = 0;
	}

	public void Free()
	{
		monsterList.Clear();
		monsterGrid.Clear();
		ActiveList.Clear();
		selectableCount = 0;
	}

	public void Update()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)combatData.monsterSpawner == (Object)null || !combatData.monsterSpawner.monsterSpawnerFinish || monsterList.Count > 3)
		{
			return;
		}
		float time = Time.time;
		foreach (zbMonster value in monsterList.Values)
		{
			Vector3 position = ((Component)value).transform.position;
			if (!(position.y < 4f) && time > value.bornTIme + 15f && !value.isEliteBoss)
			{
				EMonsterState stateId = value.fsm.GetStateId();
				bool num = stateId == EMonsterState.Move && value.GetAttrValueByType(EAttrMonsterType.MoveSpeed) <= 0f;
				bool flag = stateId != EMonsterState.Move;
				if (num | flag)
				{
					((Component)value).transform.position = new Vector3(position.x, 3f, position.z);
				}
			}
		}
	}

	public void AddMonster(zbMonster monster)
	{
		int uUID = monster.GetUUID();
		if (!monsterList.ContainsKey(uUID))
		{
			monsterList[uUID] = monster;
			monsterGrid.AddMonster(monster);
		}
		if (monster.activeIndex < 0)
		{
			monster.activeIndex = ActiveList.Count;
			ActiveList.Add(monster);
			Swap(monster.activeIndex, selectableCount);
			selectableCount++;
		}
	}

	public void RemoveMonster(zbMonster monster)
	{
		monsterList.Remove(monster.GetUUID());
		monsterGrid.RemoveMonster(monster);
		int activeIndex = monster.activeIndex;
		if (activeIndex < 0)
		{
			return;
		}
		List<zbMonster> activeList = ActiveList;
		int num = activeList.Count - 1;
		if (activeIndex <= num && !((Object)(object)activeList[activeIndex] != (Object)(object)monster))
		{
			if (activeIndex < selectableCount)
			{
				selectableCount--;
			}
			if (activeIndex != num)
			{
				zbMonster zbMonster2 = (activeList[activeIndex] = activeList[num]);
				zbMonster2.activeIndex = activeIndex;
			}
			activeList.RemoveAt(num);
			monster.activeIndex = -1;
		}
	}

	public void SetHidden(zbMonster monster)
	{
		int activeIndex = monster.activeIndex;
		if (activeIndex >= 0 && activeIndex < selectableCount)
		{
			selectableCount--;
			Swap(activeIndex, selectableCount);
		}
	}

	public void SetVisible(zbMonster monster)
	{
		int activeIndex = monster.activeIndex;
		if (activeIndex >= 0 && activeIndex >= selectableCount)
		{
			Swap(activeIndex, selectableCount);
			selectableCount++;
		}
	}

	private void Swap(int a, int b)
	{
		if (a != b)
		{
			List<zbMonster> activeList = ActiveList;
			zbMonster zbMonster2 = activeList[a];
			zbMonster zbMonster3 = (activeList[a] = activeList[b]);
			activeList[b] = zbMonster2;
			zbMonster3.activeIndex = a;
			zbMonster2.activeIndex = b;
		}
	}

	public void GetRandomMonsters(int count, List<zbMonster> result, int excludeUUID = 0)
	{
		result.Clear();
		int num = selectableCount;
		if (num == 0)
		{
			return;
		}
		if (excludeUUID != 0)
		{
			num--;
		}
		if (num <= 0)
		{
			return;
		}
		if (count > num)
		{
			count = num;
		}
		int num2 = Rand.Next(selectableCount);
		List<zbMonster> activeList = ActiveList;
		while (result.Count < count)
		{
			zbMonster zbMonster2 = activeList[num2];
			if (zbMonster2.GetUUID() != excludeUUID)
			{
				result.Add(zbMonster2);
			}
			num2++;
			if (num2 >= selectableCount)
			{
				num2 = 0;
			}
		}
	}

	public void GetRandomMonsters(int count, List<int> result, HashSet<int> ignoreList)
	{
		result.Clear();
		int num = selectableCount;
		if (num == 0)
		{
			return;
		}
		List<zbMonster> activeList = ActiveList;
		int num2 = -1;
		foreach (int ignore in ignoreList)
		{
			if (!monsterList.TryGetValue(ignore, out var value))
			{
				continue;
			}
			num2 = value.activeIndex;
			if (num2 >= 0 && num2 < num)
			{
				int num3 = num - 1;
				if (num2 != num3)
				{
					Swap(num2, num3);
				}
				num--;
			}
			else
			{
				num2 = -1;
			}
		}
		if (count > num)
		{
			count = num;
		}
		for (int i = 0; i < count; i++)
		{
			int num4 = Rand.Range(i, num);
			if (i != num4)
			{
				Swap(i, num4);
			}
			result.Add(activeList[i].GetUUID());
		}
		if (num2 >= 0)
		{
			int num5 = num;
			int num6 = selectableCount - 1;
			if (num5 != num6)
			{
				Swap(num5, num6);
			}
		}
	}

	public zbMonster GetMonsterById(int uuid)
	{
		monsterList.TryGetValue(uuid, out var value);
		return value;
	}

	public void GetMonsterListByTid(int tid, List<zbMonster> pMonsterList)
	{
		pMonsterList.Clear();
		foreach (zbMonster value in monsterList.Values)
		{
			if (value.monsterRecord.id == tid)
			{
				pMonsterList.Add(value);
			}
		}
	}

	public bool HaveMonsterByTid(int tid)
	{
		foreach (zbMonster value in monsterList.Values)
		{
			if (value.monsterRecord.id == tid)
			{
				return true;
			}
		}
		return false;
	}

	public zbMonster SpawnMonsterAtPosition(int monsterTid, Vector3 spawnPosition, Transform transformParent, bool isPvpEnemy = false)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		spawnPosition.y = Mathf.Max(spawnPosition.y, combatData.defense.GetAlertPositionY() + combatData.nodeRootPositonY);
		spawnPosition.x = Mathf.Clamp(spawnPosition.x, -3.1f + combatData.nodeRootPositonX, 3.1f + combatData.nodeRootPositonX);
		zbMonster monster = combatData.monsterPoolMgr.GetMonster(monsterTid);
		((Component)monster).transform.SetParent(transformParent);
		((Component)monster).transform.position = spawnPosition;
		((Component)monster).transform.localScale = Vector3.one;
		monster.Init(combatData, monsterTid, 0, _giveExp: false, _isRestoreData: false);
		zbCombatAtlasMerger.Instance.NotifyMonsterSpawned(monsterTid);
		if (isPvpEnemy)
		{
			zbMonsterEffect monsterSkillEffect = combatData.monsterEffectPoolMgr.GetMonsterSkillEffect(combatData.dataIndex);
			monsterSkillEffect.InitData(combatData, monster.GetUUID(), combatData.dataIndex);
			monsterSkillEffect.PlayEffectAnim("animation", loop: true);
		}
		return monster;
	}

	public bool HasMonsterInAlertRange(float alertHeight)
	{
		float alertRange = GetAlertRange(alertHeight);
		foreach (zbMonster value in monsterList.Values)
		{
			if (value.isCanSelected() && value.cachPositionY + combatData.nodeRootPositonY <= alertRange)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsInAlertRange(float posY, float height)
	{
		float alertPositionY = combatData.defense.GetAlertPositionY();
		float sceneAlertRange = combatData.GetSceneAlertRange();
		return posY <= alertPositionY + MathF.Min(height + combatData.defense.GetAlertOffsetY(), sceneAlertRange);
	}

	public float GetAlertRange(float height)
	{
		return combatData.defense.GetAlertPositionY() + MathF.Min(y: combatData.GetSceneAlertRange(), x: height + combatData.defense.GetAlertOffsetY());
	}

	public void GetRandomMonsterList(int count, List<zbMonster> pMonsterList, int excludeUUID = 0)
	{
		pMonsterList.Clear();
		if (count <= 0 || monsterList.Count == 0)
		{
			return;
		}
		int num = 0;
		Dictionary<int, zbMonster>.ValueCollection.Enumerator enumerator = monsterList.Values.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				zbMonster current = enumerator.Current;
				if (current.GetUUID() == excludeUUID || !current.isCanSelected())
				{
					continue;
				}
				num++;
				if (num <= count)
				{
					pMonsterList.Add(current);
					continue;
				}
				int num2 = Rand.Next(num);
				if (num2 < count)
				{
					pMonsterList[num2] = current;
				}
			}
		}
		finally
		{
			enumerator.Dispose();
		}
	}

	public zbMonster GetRandomMonster(int excludeUUID = 0)
	{
		zbMonster result = null;
		int num = 0;
		foreach (zbMonster value in monsterList.Values)
		{
			if (value.isCanSelected() && value.GetUUID() != excludeUUID)
			{
				num++;
				if (Rand.Next(num) == 0)
				{
					result = value;
				}
			}
		}
		return result;
	}

	public zbMonster GetRandomMonster(HashSet<int> ignoreList)
	{
		zbMonster result = null;
		int num = 0;
		foreach (zbMonster value in monsterList.Values)
		{
			if (value.isCanSelected() && !ignoreList.Contains(value.GetUUID()))
			{
				num++;
				if (Rand.Next(num) == 0)
				{
					result = value;
				}
			}
		}
		return result;
	}

	public zbMonster GetBingbaoRandomMonster(int y = 0, int excludeUUID = 0, bool again = false)
	{
		zbMonster zbMonster2 = null;
		int num = 0;
		foreach (zbMonster value in monsterList.Values)
		{
			if (value.isCanSelected() && !(value.GetCurPositionXY().y > (float)y) && value.GetUUID() != excludeUUID)
			{
				num++;
				if (Rand.Next(num) == 0)
				{
					zbMonster2 = value;
				}
			}
		}
		if ((Object)(object)zbMonster2 == (Object)null && !again)
		{
			return GetBingbaoRandomMonster(y + 10, 0, again: true);
		}
		return zbMonster2;
	}

	public void GetRangeAllMonsterList(Vector3 center, List<zbMonster> pMonsterList, float distance = 1f, int excludeUUID = 0)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		float num = distance * distance;
		pMonsterList.Clear();
		float x = center.x;
		float y = center.y;
		List<zbMonster> activeList = ActiveList;
		for (int i = 0; i < selectableCount; i++)
		{
			zbMonster zbMonster2 = activeList[i];
			if (zbMonster2.GetUUID() != excludeUUID && zbUtil.GetSplitV3SqrDistance2D(x, y, zbMonster2.cachPositionX, zbMonster2.cachPositionY) <= num)
			{
				pMonsterList.Add(zbMonster2);
			}
		}
	}

	public void GetRangeRandomMonsterList(Vector2 center, List<zbMonster> pMonsterList, float distance = 1f, int count = 1, int excludeUUID = 0)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		pMonsterList.Clear();
		if (count <= 0)
		{
			return;
		}
		int count2 = ActiveList.Count;
		if (count2 == 0)
		{
			return;
		}
		float num = distance * distance;
		float x = center.x;
		float y = center.y;
		int num2 = Rand.Next(count2);
		int num3 = 1;
		for (int i = 0; i < count2; i += num3)
		{
			int index = (num2 + i) % count2;
			zbMonster zbMonster2 = ActiveList[index];
			if (zbMonster2.GetUUID() == excludeUUID)
			{
				continue;
			}
			float num4 = x - zbMonster2.cachPositionX;
			float num5 = y - zbMonster2.cachPositionY;
			if (num4 * num4 + num5 * num5 > num)
			{
				continue;
			}
			if (pMonsterList.Count < count)
			{
				pMonsterList.Add(zbMonster2);
				if (pMonsterList.Count == count)
				{
					num3 = 5;
				}
			}
			else if (Rand.Next(2) == 0)
			{
				pMonsterList[Rand.Next(count)] = zbMonster2;
			}
		}
	}

	public zbMonster GetClosestMonster(float alertRange)
	{
		if (zbCombatSystem.Instance.m_focusFlag == 1)
		{
			zbMonster closestBossMonster = GetClosestBossMonster(alertRange);
			if ((Object)(object)closestBossMonster != (Object)null)
			{
				return closestBossMonster;
			}
		}
		float alertRange2 = GetAlertRange(alertRange);
		zbMonster result = null;
		float num = float.MaxValue;
		foreach (zbMonster value in monsterList.Values)
		{
			if (value.isCanSelected())
			{
				float cachPositionY = value.cachPositionY;
				if (cachPositionY < alertRange2 && cachPositionY < num)
				{
					result = value;
					num = cachPositionY;
				}
			}
		}
		return result;
	}

	public zbMonster GetClosestBossMonster(float alertHeight, bool ignoreUnSelect = false)
	{
		zbMonster result = null;
		float num = float.MaxValue;
		float alertRange = GetAlertRange(alertHeight);
		foreach (zbMonster value in monsterList.Values)
		{
			if (value.isEliteBoss && (ignoreUnSelect || value.isCanSelected()))
			{
				float cachPositionY = value.cachPositionY;
				if (!(cachPositionY >= alertRange) && cachPositionY < num)
				{
					result = value;
					num = cachPositionY;
				}
			}
		}
		return result;
	}

	public zbMonster GetClosestMonsterByVector(Vector3 center, HashSet<int> ints = null)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		zbMonster result = null;
		float num = float.MaxValue;
		float x = center.x;
		float y = center.y;
		foreach (zbMonster value in monsterList.Values)
		{
			if (value.isCanSelected() && (ints == null || !ints.Contains(value.GetUUID())))
			{
				float splitV3SqrDistance2D = zbUtil.GetSplitV3SqrDistance2D(x, y, value.cachPositionX, value.cachPositionY);
				if (splitV3SqrDistance2D < num)
				{
					result = value;
					num = splitV3SqrDistance2D;
				}
			}
		}
		return result;
	}
}
