using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbBuffSystem
{
	public List<zbBuffBase> buffList;

	private Dictionary<long, zbBuffBase> heroMonsterCache = new Dictionary<long, zbBuffBase>();

	private Dictionary<int, List<zbBuffBase>> monsterBuffsCache = new Dictionary<int, List<zbBuffBase>>();

	public uint buffChangeVersion { get; private set; }

	private long GetHeroMonsterBuffKey(int skillType, int monsterUUID, int buffId)
	{
		return ((long)skillType << 50) | ((long)monsterUUID << 25) | buffId;
	}

	private void AddToMonsterBuffCache(int uuid, zbBuffBase buff)
	{
		if (!monsterBuffsCache.TryGetValue(uuid, out var value))
		{
			value = new List<zbBuffBase>();
			monsterBuffsCache[uuid] = value;
		}
		value.Add(buff);
	}

	private void RemoveFromMonsterBuffCache(int uuid, zbBuffBase buff)
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
		buffList = new List<zbBuffBase>();
		heroMonsterCache.Clear();
	}

	public void Free()
	{
		buffList.Clear();
		heroMonsterCache.Clear();
		monsterBuffsCache.Clear();
		uint num = buffChangeVersion + 1;
		buffChangeVersion = num;
	}

	private void AddBuff(zbBuffBase buff)
	{
		buffList.Add(buff);
		long heroMonsterBuffKey = GetHeroMonsterBuffKey(buff.skillBaseHeroSkillType, buff.monsterUUID, buff.buffRecord.id);
		heroMonsterCache[heroMonsterBuffKey] = buff;
		AddToMonsterBuffCache(buff.monsterUUID, buff);
		uint num = buffChangeVersion + 1;
		buffChangeVersion = num;
	}

	private void RemoveBuff(int index, zbBuffBase buff)
	{
		buffList.RemoveAt(index);
		long heroMonsterBuffKey = GetHeroMonsterBuffKey(buff.skillBaseHeroSkillType, buff.monsterUUID, buff.buffRecord.id);
		heroMonsterCache.Remove(heroMonsterBuffKey);
		RemoveFromMonsterBuffCache(buff.monsterUUID, buff);
		uint num = buffChangeVersion + 1;
		buffChangeVersion = num;
	}

	public void Update()
	{
		for (int num = buffList.Count - 1; num >= 0; num--)
		{
			zbBuffBase zbBuffBase2 = buffList[num];
			zbBuffBase2.Update();
			if (zbBuffBase2.IsEnd())
			{
				zbBuffBase2.Free();
				RemoveBuff(num, zbBuffBase2);
			}
		}
	}

	public void CreateBuff(zbSkillBase skillBase, Dictionary<EAttrSkillType, float> skillAttrList, int monsterUUID, int buffId, float _extraTime = 0f, int _layer = 1, float _param1 = 0f, float _param2 = 0f)
	{
		zbMonster monsterById = skillBase.combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById == (Object)null || monsterById.immAllBuff > 0)
		{
			return;
		}
		zbSkillBuffCsvRecord recordByKey = zbDataConfig.Instance.skillBuffCsvTable.GetRecordByKey(buffId);
		if (recordByKey == null || monsterById.immBuffList.Contains(recordByKey.buffIndex) || (monsterById.isEliteBoss && recordByKey.bossImmune == 1))
		{
			return;
		}
		foreach (stMonsterSkillIndex10001_2 item in monsterById.monsterIndex10001_2)
		{
			if (item.buffId == buffId && item.immTime > (float)zbCombatSystem.Instance.accumTime)
			{
				return;
			}
		}
		if (monsterById.monsterSkillIndex3011 > 0)
		{
			if ((monsterById.monsterSkillIndex3011 != 1 || !monsterById.monster30111Class.actioning) && (monsterById.monsterSkillIndex3011 != 2 || !monsterById.monster30112Class.actioning) && monsterById.monsterSkillIndex3011 == 3)
			{
				_ = monsterById.monster30113Class.actioning;
			}
		}
		else
		{
			if (((monsterById.monsterSkillIndex3011 == 1 || monsterById.monsterSkillIndex3011 == 2) && monsterById.monsterSkillIndex30111.immBuffs.Contains(buffId)) || (monsterById.monsterSkillIndex3012 == 3 && monsterById.monsterSkillIndex30123.immBuffs.Contains(buffId)))
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
			zbBuffBase zbBuffBase2 = IsHeroMonsterHaveBuff(skillBase.hero.GetSkillType(), monsterUUID, buffId);
			if (zbBuffBase2 != null)
			{
				if (recordByKey.layer > 0)
				{
					zbBuffBase2.AddLayer(_layer);
				}
				else
				{
					zbBuffBase2.Reset();
					zbBuffBase2.Init(skillBase, skillAttrList, monsterUUID, recordByKey, _extraTime, _param1, _param2);
				}
			}
			else
			{
				zbBuffBase2 = zbSkillSystem.Instance.buffRegister.CreateBuff(buffId);
				if (zbBuffBase2 == null)
				{
					return;
				}
				zbBuffBase2.Init(skillBase, skillAttrList, monsterUUID, recordByKey, _extraTime, _param1, _param2);
				zbBuffBase2.AddLayer(_layer);
				AddBuff(zbBuffBase2);
			}
			if (!(num > 0f) || zbBuffBase2 == null)
			{
				return;
			}
			bool flag = false;
			foreach (stMonsterSkillIndex10001_2 item2 in monsterById.monsterIndex10001_2)
			{
				if (item2.buffId == buffId)
				{
					item2.immTime = (float)zbCombatSystem.Instance.accumTime + num + zbBuffBase2.durationTime;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				stMonsterSkillIndex10001_2 stMonsterSkillIndex10001_3 = new stMonsterSkillIndex10001_2();
				stMonsterSkillIndex10001_3.buffId = buffId;
				stMonsterSkillIndex10001_3.immTime = (float)zbCombatSystem.Instance.accumTime + num + zbBuffBase2.durationTime;
				monsterById.monsterIndex10001_2.Add(stMonsterSkillIndex10001_3);
			}
		}
	}

	public zbBuffBase IsHeroMonsterHaveBuff(int skillType, int monsterUUID, int buffId)
	{
		long heroMonsterBuffKey = GetHeroMonsterBuffKey(skillType, monsterUUID, buffId);
		if (heroMonsterCache.TryGetValue(heroMonsterBuffKey, out var value))
		{
			return value;
		}
		return null;
	}

	public bool IsMonsterHaveAnyBuff(int monsterUUID)
	{
		if (monsterBuffsCache.TryGetValue(monsterUUID, out var value))
		{
			return value.Count > 0;
		}
		return false;
	}

	public zbBuffBase IsMonsterHaveBuff(int monsterUUID, int buffId = 0)
	{
		if (monsterBuffsCache.TryGetValue(monsterUUID, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				if (buffId == 0 || value[i].buffRecord.id == buffId)
				{
					return value[i];
				}
			}
		}
		return null;
	}

	public bool IsMonsterHaveBuffs(int monsterUUID, int buffId1, int buffId2, int buffId3)
	{
		if (monsterBuffsCache.TryGetValue(monsterUUID, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				int id = value[i].buffRecord.id;
				if (id == buffId1 || id == buffId2 || id == buffId3)
				{
					return true;
				}
			}
		}
		return false;
	}

	public zbBuffBase IsMonsterHaveSubSkill(int subSkillId, int monsterUUID)
	{
		if (monsterBuffsCache.TryGetValue(monsterUUID, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				zbBuffBase zbBuffBase2 = value[i];
				if (zbBuffBase2.skillBase.isSubSkill && zbBuffBase2.skillBase.skillSubRecord.id == subSkillId)
				{
					return zbBuffBase2;
				}
			}
		}
		return null;
	}

	public void RemoveBuffInDistance(Vector3 vector, float distance)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		float num = distance * distance;
		float x = vector.x;
		float y = vector.y;
		for (int num2 = buffList.Count - 1; num2 >= 0; num2--)
		{
			zbBuffBase zbBuffBase2 = buffList[num2];
			zbMonster monsterById = zbBuffBase2.skillBase.combatData.monsters.GetMonsterById(zbBuffBase2.monsterUUID);
			if ((Object)(object)monsterById != (Object)null && zbUtil.GetSplitV3SqrDistance2D(x, y, monsterById.cachPositionX, monsterById.cachPositionY) <= num)
			{
				zbBuffBase2.Free();
				RemoveBuff(num2, zbBuffBase2);
			}
		}
	}
}
