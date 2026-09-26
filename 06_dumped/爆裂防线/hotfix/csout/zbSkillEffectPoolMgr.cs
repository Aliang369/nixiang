using System.Collections.Generic;
using UnityEngine;

public class zbSkillEffectPoolMgr
{
	private Dictionary<int, zbSkillEffectPool> m_pool;

	private Transform m_poolRoot;

	private int m_activeCount;

	private int m_activePeak;

	private const string kCounterActive = "技能特效池活跃数";

	private const string kCounterPeak = "技能特效池活跃峰值";

	private void OnPoolGet()
	{
		m_activeCount++;
		if (m_activeCount > m_activePeak)
		{
			m_activePeak = m_activeCount;
			zbPerfLog.Set("技能特效池活跃峰值", m_activePeak);
		}
		zbPerfLog.Set("技能特效池活跃数", m_activeCount);
	}

	private void OnPoolReturn()
	{
		if (m_activeCount > 0)
		{
			m_activeCount--;
		}
		zbPerfLog.Set("技能特效池活跃数", m_activeCount);
	}

	public void Init(Transform nodeRoot)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		m_pool = new Dictionary<int, zbSkillEffectPool>();
		m_poolRoot = new GameObject("RootSkillEffectPool").transform;
		m_poolRoot.SetParent(nodeRoot, false);
	}

	public void Free()
	{
		foreach (zbSkillEffectPool value in m_pool.Values)
		{
			value.Free();
		}
		m_pool.Clear();
	}

	public zbSkillEffect GetEffect(int effectId)
	{
		if (zbDataConfig.Instance.skillEffectCsvTable.GetRecordByKey(effectId) == null)
		{
			return null;
		}
		if (m_pool.ContainsKey(effectId))
		{
			zbSkillEffect effect = m_pool[effectId].GetEffect();
			if ((Object)(object)effect != (Object)null)
			{
				OnPoolGet();
			}
			return effect;
		}
		zbSkillEffectPool zbSkillEffectPool2 = new zbSkillEffectPool();
		zbSkillEffectPool2.Init(effectId, m_poolRoot);
		m_pool.Add(effectId, zbSkillEffectPool2);
		zbSkillEffect effect2 = zbSkillEffectPool2.GetEffect();
		if ((Object)(object)effect2 != (Object)null)
		{
			OnPoolGet();
		}
		return effect2;
	}

	public void ReturnEffect(zbSkillEffect effect)
	{
		if (m_pool.ContainsKey(effect.skillEffectRecord.id))
		{
			m_pool[effect.skillEffectRecord.id].ReturnEffect(effect);
			OnPoolReturn();
		}
	}

	public void CollectActiveSpineZ(List<float> results)
	{
		foreach (zbSkillEffectPool value in m_pool.Values)
		{
			value.CollectActiveSpineZ(results);
		}
	}
}
