using System.Collections.Generic;
using UnityEngine;

public class zbMonsterOnceEffectPoolMgr
{
	private Dictionary<int, zbMonsterOnceEffectPool> m_pool;

	private Transform m_poolRoot;

	private int m_activeCount;

	private int m_activePeak;

	private const string kCounterActive = "一次性特效池活跃数";

	private const string kCounterPeak = "一次性特效池活跃峰值";

	private void OnPoolGet()
	{
		m_activeCount++;
		if (m_activeCount > m_activePeak)
		{
			m_activePeak = m_activeCount;
			zbPerfLog.Set("一次性特效池活跃峰值", m_activePeak);
		}
		zbPerfLog.Set("一次性特效池活跃数", m_activeCount);
	}

	private void OnPoolReturn()
	{
		if (m_activeCount > 0)
		{
			m_activeCount--;
		}
		zbPerfLog.Set("一次性特效池活跃数", m_activeCount);
	}

	public void Init(Transform nodeRoot)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		m_pool = new Dictionary<int, zbMonsterOnceEffectPool>();
		m_poolRoot = new GameObject("RootOnceEffectPool").transform;
		m_poolRoot.SetParent(nodeRoot, false);
	}

	public void Free()
	{
		foreach (zbMonsterOnceEffectPool value in m_pool.Values)
		{
			value.Free();
		}
		m_pool.Clear();
	}

	public zbMonsterOnceEffect GetMonsterSkillEffect(int monsterEffectId)
	{
		if (zbDataConfig.Instance.monsterEffectCsvTable.GetRecordByKey(monsterEffectId) == null)
		{
			return null;
		}
		if (m_pool.ContainsKey(monsterEffectId))
		{
			zbMonsterOnceEffect monsterSkillEffect = m_pool[monsterEffectId].GetMonsterSkillEffect();
			if ((Object)(object)monsterSkillEffect != (Object)null)
			{
				OnPoolGet();
			}
			return monsterSkillEffect;
		}
		zbMonsterOnceEffectPool zbMonsterOnceEffectPool2 = new zbMonsterOnceEffectPool();
		zbMonsterOnceEffectPool2.Init(monsterEffectId, m_poolRoot);
		m_pool.Add(monsterEffectId, zbMonsterOnceEffectPool2);
		zbMonsterOnceEffect monsterSkillEffect2 = zbMonsterOnceEffectPool2.GetMonsterSkillEffect();
		if ((Object)(object)monsterSkillEffect2 != (Object)null)
		{
			OnPoolGet();
		}
		return monsterSkillEffect2;
	}

	public void ReturnMonsterSkillEffect(zbMonsterOnceEffect monsterSkill)
	{
		if (m_pool.ContainsKey(monsterSkill.monsterEffectRecord.id))
		{
			m_pool[monsterSkill.monsterEffectRecord.id].ReturnMonsterSkillEffect(monsterSkill);
			OnPoolReturn();
		}
	}

	public void CollectActiveSpineZ(List<float> results)
	{
		foreach (zbMonsterOnceEffectPool value in m_pool.Values)
		{
			value.CollectActiveSpineZ(results);
		}
	}
}
