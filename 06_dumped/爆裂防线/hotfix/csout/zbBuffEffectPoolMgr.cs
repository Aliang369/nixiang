using System.Collections.Generic;
using UnityEngine;

public class zbBuffEffectPoolMgr
{
	private Dictionary<int, zbBuffEffectPool> m_pool;

	private Transform m_poolRoot;

	private int m_activeCount;

	private int m_activePeak;

	private const string kCounterActive = "buff特效池活跃数";

	private const string kCounterPeak = "buff特效池活跃峰值";

	private void OnPoolGet()
	{
		m_activeCount++;
		if (m_activeCount > m_activePeak)
		{
			m_activePeak = m_activeCount;
			zbPerfLog.Set("buff特效池活跃峰值", m_activePeak);
		}
		zbPerfLog.Set("buff特效池活跃数", m_activeCount);
	}

	private void OnPoolReturn()
	{
		if (m_activeCount > 0)
		{
			m_activeCount--;
		}
		zbPerfLog.Set("buff特效池活跃数", m_activeCount);
	}

	public void Init(Transform nodeRoot)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		m_pool = new Dictionary<int, zbBuffEffectPool>();
		m_poolRoot = new GameObject("RootBuffEffectPool").transform;
		m_poolRoot.SetParent(nodeRoot, false);
	}

	public void Free()
	{
		foreach (zbBuffEffectPool value in m_pool.Values)
		{
			value.Free();
		}
		m_pool.Clear();
	}

	public zbBuffEffect GetBuffEffect(int buffTid)
	{
		if (zbDataConfig.Instance.skillBuffCsvTable.GetRecordByKey(buffTid) == null)
		{
			return null;
		}
		if (m_pool.ContainsKey(buffTid))
		{
			zbBuffEffect buffEffect = m_pool[buffTid].GetBuffEffect();
			if ((Object)(object)buffEffect != (Object)null)
			{
				OnPoolGet();
			}
			return buffEffect;
		}
		zbBuffEffectPool zbBuffEffectPool2 = new zbBuffEffectPool();
		zbBuffEffectPool2.Init(buffTid, m_poolRoot);
		m_pool.Add(buffTid, zbBuffEffectPool2);
		zbBuffEffect buffEffect2 = zbBuffEffectPool2.GetBuffEffect();
		if ((Object)(object)buffEffect2 != (Object)null)
		{
			OnPoolGet();
		}
		return buffEffect2;
	}

	public void ReturnBuffEffect(zbBuffEffect buff)
	{
		if (m_pool.ContainsKey(buff.buffRecord.id))
		{
			m_pool[buff.buffRecord.id].ReturnBuffEffect(buff);
			OnPoolReturn();
		}
	}

	public void CollectActiveSpineZ(List<float> results)
	{
		foreach (zbBuffEffectPool value in m_pool.Values)
		{
			value.CollectActiveSpineZ(results);
		}
	}
}
