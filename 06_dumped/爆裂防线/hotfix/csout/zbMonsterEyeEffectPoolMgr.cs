using System.Collections.Generic;
using UnityEngine;

public class zbMonsterEyeEffectPoolMgr
{
	private LinkedList<zbMonsterEyeEffect> m_pool;

	private LinkedList<zbMonsterEyeEffect> m_activeMonsters;

	private Transform m_poolRoot;

	private const int kExpandBatchCap = 16;

	private const int kExpandBatchMin = 4;

	private int m_activeCount;

	private int m_activePeak;

	private const string kCounterActive = "眼睛特效池活跃数";

	private const string kCounterPeak = "眼睛特效池活跃峰值";

	private void OnPoolGet()
	{
		m_activeCount++;
		if (m_activeCount > m_activePeak)
		{
			m_activePeak = m_activeCount;
			zbPerfLog.Set("眼睛特效池活跃峰值", m_activePeak);
		}
		zbPerfLog.Set("眼睛特效池活跃数", m_activeCount);
	}

	private void OnPoolReturn()
	{
		if (m_activeCount > 0)
		{
			m_activeCount--;
		}
		zbPerfLog.Set("眼睛特效池活跃数", m_activeCount);
	}

	public void Init(Transform nodeRoot)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		m_pool = new LinkedList<zbMonsterEyeEffect>();
		m_activeMonsters = new LinkedList<zbMonsterEyeEffect>();
		m_poolRoot = new GameObject("RootEyeEffectPool").transform;
		m_poolRoot.SetParent(nodeRoot, false);
	}

	public void CollectActiveSpineZ(List<float> results)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		foreach (zbMonsterEyeEffect activeMonster in m_activeMonsters)
		{
			if (!((Object)(object)activeMonster == (Object)null) && !activeMonster.m_spineGpuTakenOver && !((Object)(object)activeMonster.skeletonAnimation == (Object)null))
			{
				Transform transform = ((Component)activeMonster.skeletonAnimation).transform;
				if (!((Object)(object)transform == (Object)null) && ((Component)transform).gameObject.activeInHierarchy)
				{
					results.Add(transform.position.z);
				}
			}
		}
	}

	public void Free()
	{
		m_pool.Clear();
		m_activeMonsters.Clear();
	}

	public zbMonsterEyeEffect GetMonsterEyeEffect()
	{
		zbMonsterEyeEffect zbMonsterEyeEffect2 = null;
		if (m_pool.Count == 0)
		{
			ExpandPool(CalcExpandBatch());
		}
		if (m_pool.Count > 0)
		{
			zbMonsterEyeEffect2 = m_pool.First.Value;
			m_pool.RemoveFirst();
			if (!((Component)zbMonsterEyeEffect2).gameObject.activeSelf)
			{
				((Component)zbMonsterEyeEffect2).gameObject.SetActive(true);
			}
			m_activeMonsters.AddLast(zbMonsterEyeEffect2);
			OnPoolGet();
			return zbMonsterEyeEffect2;
		}
		return null;
	}

	public void ReturnMonsterEyeEffect(zbMonsterEyeEffect monsterEye)
	{
		if (!((Object)(object)monsterEye == (Object)null))
		{
			((Component)monsterEye).gameObject.SetActive(false);
			m_activeMonsters.Remove(monsterEye);
			m_pool.AddLast(monsterEye);
			OnPoolReturn();
		}
	}

	public void PreHeat(int count)
	{
		if (count > 0)
		{
			ExpandPool(count);
		}
	}

	private int CalcExpandBatch()
	{
		int num = (m_pool.Count + m_activeMonsters.Count + 1) / 2;
		if (num < 4)
		{
			num = 4;
		}
		if (num > 16)
		{
			num = 16;
		}
		return num;
	}

	private void ExpandPool(int count)
	{
		GameObject val = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/combat/monsterEyeEffect.prefab");
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			GameObject val2 = Object.Instantiate<GameObject>(val, m_poolRoot);
			zbMonsterEyeEffect component = val2.GetComponent<zbMonsterEyeEffect>();
			if ((Object)(object)component == (Object)null)
			{
				break;
			}
			val2.SetActive(false);
			m_pool.AddLast(component);
		}
	}
}
