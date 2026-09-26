using System.Collections.Generic;
using UnityEngine;

public class zbMonsterDieEffectPoolMgr
{
	private LinkedList<zbMonsterDieEffect> m_pool;

	private LinkedList<zbMonsterDieEffect> m_activeMonsters;

	private Transform m_poolRoot;

	private const int kExpandBatchCap = 16;

	private const int kExpandBatchMin = 4;

	public void Init(Transform nodeRoot)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		m_pool = new LinkedList<zbMonsterDieEffect>();
		m_activeMonsters = new LinkedList<zbMonsterDieEffect>();
		m_poolRoot = new GameObject("RootDieEffectPool").transform;
		m_poolRoot.SetParent(nodeRoot, false);
	}

	public void Free()
	{
		m_pool.Clear();
		m_activeMonsters.Clear();
	}

	public zbMonsterDieEffect GetMonsterDieEffect()
	{
		zbMonsterDieEffect zbMonsterDieEffect2 = null;
		if (m_pool.Count == 0)
		{
			ExpandPool(CalcExpandBatch());
		}
		if (m_pool.Count > 0)
		{
			zbMonsterDieEffect2 = m_pool.First.Value;
			m_pool.RemoveFirst();
			if (!((Component)zbMonsterDieEffect2).gameObject.activeSelf)
			{
				((Component)zbMonsterDieEffect2).gameObject.SetActive(true);
			}
			m_activeMonsters.AddLast(zbMonsterDieEffect2);
			return zbMonsterDieEffect2;
		}
		return null;
	}

	public void ReturnMonsterDieEffect(zbMonsterDieEffect monsterDie)
	{
		if (!((Object)(object)monsterDie == (Object)null))
		{
			((Component)monsterDie).gameObject.SetActive(false);
			m_activeMonsters.Remove(monsterDie);
			m_pool.AddLast(monsterDie);
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
		GameObject val = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/combat/monsterDieEffect.prefab");
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			GameObject val2 = Object.Instantiate<GameObject>(val, m_poolRoot);
			zbMonsterDieEffect component = val2.GetComponent<zbMonsterDieEffect>();
			if ((Object)(object)component == (Object)null)
			{
				break;
			}
			val2.SetActive(false);
			m_pool.AddLast(component);
		}
	}
}
