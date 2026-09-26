using System.Collections.Generic;
using UnityEngine;

public class zbBuffEffectPool
{
	private LinkedList<zbBuffEffect> m_pool;

	private LinkedList<zbBuffEffect> m_activeMonsters;

	private Transform m_poolRoot;

	private const int kExpandBatchCap = 16;

	private const int kExpandBatchMin = 4;

	public void Init(int buffId, Transform poolRoot)
	{
		m_poolRoot = poolRoot;
		m_pool = new LinkedList<zbBuffEffect>();
		m_activeMonsters = new LinkedList<zbBuffEffect>();
	}

	public void Free()
	{
		m_pool.Clear();
		m_activeMonsters.Clear();
	}

	public zbBuffEffect GetBuffEffect()
	{
		zbBuffEffect zbBuffEffect2 = null;
		if (m_pool.Count == 0)
		{
			ExpandPool(CalcExpandBatch());
		}
		if (m_pool.Count > 0)
		{
			zbBuffEffect2 = m_pool.First.Value;
			m_pool.RemoveFirst();
			if (!((Component)zbBuffEffect2).gameObject.activeSelf)
			{
				((Component)zbBuffEffect2).gameObject.SetActive(true);
			}
			m_activeMonsters.AddLast(zbBuffEffect2);
			return zbBuffEffect2;
		}
		return null;
	}

	public void ReturnBuffEffect(zbBuffEffect monster)
	{
		if (((Component)monster).gameObject.activeSelf)
		{
			((Component)monster).gameObject.SetActive(false);
			m_activeMonsters.Remove(monster);
			m_pool.AddLast(monster);
		}
	}

	public void CollectActiveSpineZ(List<float> results)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		foreach (zbBuffEffect activeMonster in m_activeMonsters)
		{
			if (!((Object)(object)activeMonster == (Object)null) && !((Object)(object)activeMonster.skeletonAnimation == (Object)null) && (!activeMonster.m_spineGpuTakenOver || activeMonster.isSpriteReplaced))
			{
				Transform val = (activeMonster.isSpriteReplaced ? ((Component)activeMonster).transform : ((Component)activeMonster.skeletonAnimation).transform);
				if (!((Object)(object)val == (Object)null) && ((Component)val).gameObject.activeInHierarchy)
				{
					results.Add(val.position.z);
				}
			}
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
		GameObject val = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/combat/buffEffect.prefab");
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			GameObject val2 = Object.Instantiate<GameObject>(val, m_poolRoot);
			zbBuffEffect component = val2.GetComponent<zbBuffEffect>();
			if ((Object)(object)component == (Object)null)
			{
				break;
			}
			val2.SetActive(false);
			m_pool.AddLast(component);
		}
	}
}
