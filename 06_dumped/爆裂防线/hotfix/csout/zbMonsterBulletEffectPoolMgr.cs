using System.Collections.Generic;
using UnityEngine;

public class zbMonsterBulletEffectPoolMgr
{
	private Dictionary<int, zbMonsterBulletEffectPool> m_pool;

	private Transform m_poolRoot;

	private int m_activeCount;

	private int m_activePeak;

	private const string kCounterActive = "弹幕池活跃数";

	private const string kCounterPeak = "弹幕池活跃峰值";

	private void OnPoolGet()
	{
		m_activeCount++;
		if (m_activeCount > m_activePeak)
		{
			m_activePeak = m_activeCount;
			zbPerfLog.Set("弹幕池活跃峰值", m_activePeak);
		}
		zbPerfLog.Set("弹幕池活跃数", m_activeCount);
	}

	private void OnPoolReturn()
	{
		if (m_activeCount > 0)
		{
			m_activeCount--;
		}
		zbPerfLog.Set("弹幕池活跃数", m_activeCount);
	}

	public void Init(Transform nodeRoot)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		m_pool = new Dictionary<int, zbMonsterBulletEffectPool>();
		m_poolRoot = new GameObject("RootBulletEffectPool").transform;
		m_poolRoot.SetParent(nodeRoot, false);
	}

	public void Free()
	{
		foreach (zbMonsterBulletEffectPool value in m_pool.Values)
		{
			value.Free();
		}
		m_pool.Clear();
	}

	public zbMonsterBullet GetMonsterBulletEffect(int monsterTid)
	{
		if (zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(monsterTid) == null)
		{
			return null;
		}
		zbMonsterBullet monsterBulletEffect = EnsurePool(monsterTid).GetMonsterBulletEffect();
		if ((Object)(object)monsterBulletEffect != (Object)null)
		{
			OnPoolGet();
		}
		return monsterBulletEffect;
	}

	public void PreHeat(int monsterTid, int count)
	{
		if (count > 0 && zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(monsterTid) != null)
		{
			EnsurePool(monsterTid).PreHeat(count);
		}
	}

	private zbMonsterBulletEffectPool EnsurePool(int monsterTid)
	{
		if (!m_pool.ContainsKey(monsterTid))
		{
			zbMonsterBulletEffectPool zbMonsterBulletEffectPool2 = new zbMonsterBulletEffectPool();
			zbMonsterBulletEffectPool2.Init(monsterTid, m_poolRoot);
			m_pool.Add(monsterTid, zbMonsterBulletEffectPool2);
		}
		return m_pool[monsterTid];
	}

	public void ReturnMonsterBulletEffect(zbMonsterBullet monsterBullet)
	{
		if (m_pool.ContainsKey(monsterBullet.monsterRecord.id))
		{
			m_pool[monsterBullet.monsterRecord.id].ReturnMonsterBulletEffect(monsterBullet);
			OnPoolReturn();
		}
	}

	public void CollectActiveSpineZ(List<float> results)
	{
		foreach (zbMonsterBulletEffectPool value in m_pool.Values)
		{
			value.CollectActiveSpineZ(results);
		}
	}
}
