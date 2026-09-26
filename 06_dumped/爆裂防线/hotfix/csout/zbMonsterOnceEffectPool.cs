using System.Collections.Generic;
using UnityEngine;

public class zbMonsterOnceEffectPool
{
	private LinkedList<zbMonsterOnceEffect> m_pool;

	private LinkedList<zbMonsterOnceEffect> m_activeMonsters;

	private Transform m_poolRoot;

	private const int kExpandBatchCap = 16;

	private const int kExpandBatchMin = 4;

	private int monsterSkillId;

	public void Init(int _monsterSkillId, Transform poolRoot)
	{
		monsterSkillId = _monsterSkillId;
		m_poolRoot = poolRoot;
		m_pool = new LinkedList<zbMonsterOnceEffect>();
		m_activeMonsters = new LinkedList<zbMonsterOnceEffect>();
	}

	public void Free()
	{
		m_pool.Clear();
		m_activeMonsters.Clear();
	}

	public zbMonsterOnceEffect GetMonsterSkillEffect()
	{
		zbMonsterOnceEffect zbMonsterOnceEffect2 = null;
		if (m_pool.Count == 0)
		{
			ExpandPool(CalcExpandBatch());
		}
		if (m_pool.Count > 0)
		{
			zbMonsterOnceEffect2 = m_pool.First.Value;
			m_pool.RemoveFirst();
			if (!((Component)zbMonsterOnceEffect2).gameObject.activeSelf)
			{
				((Component)zbMonsterOnceEffect2).gameObject.SetActive(true);
			}
			m_activeMonsters.AddLast(zbMonsterOnceEffect2);
			return zbMonsterOnceEffect2;
		}
		return null;
	}

	public void ReturnMonsterSkillEffect(zbMonsterOnceEffect monster)
	{
		if (!((Object)(object)monster == (Object)null))
		{
			((Component)monster).gameObject.SetActive(false);
			m_activeMonsters.Remove(monster);
			m_pool.AddLast(monster);
		}
	}

	public void CollectActiveSpineZ(List<float> results)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		foreach (zbMonsterOnceEffect activeMonster in m_activeMonsters)
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
		GameObject val = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/combat/monsterOnceEffect.prefab");
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			GameObject val2 = Object.Instantiate<GameObject>(val, m_poolRoot);
			zbMonsterOnceEffect component = val2.GetComponent<zbMonsterOnceEffect>();
			if ((Object)(object)component == (Object)null)
			{
				break;
			}
			val2.SetActive(false);
			m_pool.AddLast(component);
		}
	}
}
