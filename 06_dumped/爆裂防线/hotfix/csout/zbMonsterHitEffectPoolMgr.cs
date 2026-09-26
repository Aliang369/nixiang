using System.Collections.Generic;
using UnityEngine;

public class zbMonsterHitEffectPoolMgr
{
	private LinkedList<zbMonsterHitEffect> m_pool;

	private LinkedList<zbMonsterHitEffect> m_activeMonsters;

	private int m_maxActiveCount = 20;

	private Transform m_poolRoot;

	private const int kExpandBatchCap = 16;

	private const int kExpandBatchMin = 4;

	public void Init(Transform nodeRoot, int maxActiveCount = 20)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		m_pool = new LinkedList<zbMonsterHitEffect>();
		m_activeMonsters = new LinkedList<zbMonsterHitEffect>();
		m_maxActiveCount = maxActiveCount;
		m_poolRoot = new GameObject("RootHitEffectPool").transform;
		m_poolRoot.SetParent(nodeRoot, false);
	}

	public void Free()
	{
		m_pool.Clear();
		m_activeMonsters.Clear();
	}

	public zbMonsterHitEffect GetMonsteHitEffect()
	{
		if (m_activeMonsters.Count >= m_maxActiveCount)
		{
			return null;
		}
		zbMonsterHitEffect zbMonsterHitEffect2 = null;
		if (m_pool.Count == 0)
		{
			ExpandPool(CalcExpandBatch());
		}
		if (m_pool.Count > 0)
		{
			zbMonsterHitEffect2 = m_pool.First.Value;
			m_pool.RemoveFirst();
			if (!((Component)zbMonsterHitEffect2).gameObject.activeSelf)
			{
				((Component)zbMonsterHitEffect2).gameObject.SetActive(true);
			}
			m_activeMonsters.AddLast(zbMonsterHitEffect2);
			return zbMonsterHitEffect2;
		}
		return null;
	}

	public void ReturnMonsterHitEffect(zbMonsterHitEffect monsterHit)
	{
		if (!((Object)(object)monsterHit == (Object)null))
		{
			((Component)monsterHit).gameObject.SetActive(false);
			m_activeMonsters.Remove(monsterHit);
			m_pool.AddLast(monsterHit);
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
		GameObject val = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/combat/monsterHitEffect.prefab");
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			GameObject val2 = Object.Instantiate<GameObject>(val, m_poolRoot);
			zbMonsterHitEffect component = val2.GetComponent<zbMonsterHitEffect>();
			if ((Object)(object)component == (Object)null)
			{
				break;
			}
			val2.SetActive(false);
			m_pool.AddLast(component);
		}
	}
}
