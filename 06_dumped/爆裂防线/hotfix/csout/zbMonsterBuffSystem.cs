using System.Collections.Generic;
using UnityEngine;

public class zbMonsterBuffSystem
{
	public List<zbMonsterBuffBase> buffList;

	private Dictionary<long, zbMonsterBuffBase> heroMonsterCache = new Dictionary<long, zbMonsterBuffBase>();

	private Dictionary<int, List<zbMonsterBuffBase>> monsterBuffsCache = new Dictionary<int, List<zbMonsterBuffBase>>();

	private long GetHeroMonsterBuffKey(int monsterUUID, int buffId)
	{
		return ((long)monsterUUID << 25) | buffId;
	}

	private void AddToMonsterBuffCache(int uuid, zbMonsterBuffBase buff)
	{
		if (!monsterBuffsCache.TryGetValue(uuid, out var value))
		{
			value = new List<zbMonsterBuffBase>();
			monsterBuffsCache[uuid] = value;
		}
		value.Add(buff);
	}

	private void RemoveFromMonsterBuffCache(int uuid, zbMonsterBuffBase buff)
	{
		if (monsterBuffsCache.TryGetValue(uuid, out var value))
		{
			value.Remove(buff);
			if (value.Count == 0)
			{
				monsterBuffsCache.Remove(uuid);
			}
		}
	}

	public void Init()
	{
		buffList = new List<zbMonsterBuffBase>();
		heroMonsterCache.Clear();
	}

	public void Free()
	{
		buffList.Clear();
		heroMonsterCache.Clear();
		monsterBuffsCache.Clear();
	}

	private void AddBuff(zbMonsterBuffBase buff)
	{
		buffList.Add(buff);
		long heroMonsterBuffKey = GetHeroMonsterBuffKey(buff.monsterUUID, buff.buffRecord.id);
		heroMonsterCache[heroMonsterBuffKey] = buff;
		AddToMonsterBuffCache(buff.monsterUUID, buff);
	}

	private void RemoveBuff(int index, zbMonsterBuffBase buff)
	{
		buffList.RemoveAt(index);
		long heroMonsterBuffKey = GetHeroMonsterBuffKey(buff.monsterUUID, buff.buffRecord.id);
		heroMonsterCache.Remove(heroMonsterBuffKey);
		RemoveFromMonsterBuffCache(buff.monsterUUID, buff);
	}

	public void Update()
	{
		for (int num = buffList.Count - 1; num >= 0; num--)
		{
			zbMonsterBuffBase zbMonsterBuffBase2 = buffList[num];
			zbMonsterBuffBase2.Update();
			if (zbMonsterBuffBase2.IsEnd())
			{
				zbMonsterBuffBase2.Free();
				RemoveBuff(num, zbMonsterBuffBase2);
			}
		}
	}

	public void CreateBuff(zbMonster pMonster, int monsterUUID, int buffId, float _extraTime = 0f, int _layer = 1)
	{
		zbMonster monsterById = pMonster.combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById == (Object)null)
		{
			return;
		}
		zbMonsterBuffCsvRecord recordByKey = zbDataConfig.Instance.monsterBuffCsvTable.GetRecordByKey(buffId);
		if (recordByKey == null)
		{
			return;
		}
		float num = 0f;
		foreach (int buffId2 in monsterById.monsterIndex10001.buffIdList)
		{
			if (buffId2 == buffId)
			{
				num = monsterById.monsterIndex10001.immTime;
				break;
			}
		}
		zbMonsterBuffBase zbMonsterBuffBase2 = IsHeroMonsterHaveBuff(monsterUUID, buffId);
		if (zbMonsterBuffBase2 != null)
		{
			if (recordByKey.layer > 0)
			{
				zbMonsterBuffBase2.AddLayer(_layer);
			}
			else
			{
				zbMonsterBuffBase2.Reset();
				zbMonsterBuffBase2.Init(pMonster, monsterUUID, recordByKey, _extraTime);
			}
		}
		else
		{
			zbMonsterBuffBase2 = zbSkillSystem.Instance.monsterBuffRegister.CreateBuff(buffId);
			if (zbMonsterBuffBase2 == null)
			{
				return;
			}
			zbMonsterBuffBase2.Init(pMonster, monsterUUID, recordByKey, _extraTime);
			zbMonsterBuffBase2.AddLayer(_layer);
			AddBuff(zbMonsterBuffBase2);
		}
		if (!(num > 0f) || zbMonsterBuffBase2 == null)
		{
			return;
		}
		bool flag = false;
		foreach (stMonsterSkillIndex10001_2 item in monsterById.monsterIndex10001_2)
		{
			if (item.buffId == buffId)
			{
				item.immTime = (float)zbCombatSystem.Instance.accumTime + num + zbMonsterBuffBase2.durationTime;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			stMonsterSkillIndex10001_2 stMonsterSkillIndex10001_3 = new stMonsterSkillIndex10001_2();
			stMonsterSkillIndex10001_3.buffId = buffId;
			stMonsterSkillIndex10001_3.immTime = (float)zbCombatSystem.Instance.accumTime + num + zbMonsterBuffBase2.durationTime;
			monsterById.monsterIndex10001_2.Add(stMonsterSkillIndex10001_3);
		}
	}

	public zbMonsterBuffBase IsHeroMonsterHaveBuff(int monsterUUID, int buffId)
	{
		long heroMonsterBuffKey = GetHeroMonsterBuffKey(monsterUUID, buffId);
		if (heroMonsterCache.TryGetValue(heroMonsterBuffKey, out var value))
		{
			return value;
		}
		return null;
	}
}
