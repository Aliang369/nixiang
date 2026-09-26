using System.Collections.Generic;
using UnityEngine;

public class zbMonsterEffectPool
{
	private LinkedList<zbMonsterEffect> m_pool;

	private LinkedList<zbMonsterEffect> m_activeMonsters;

	private Transform m_poolRoot;

	private const int kExpandBatchCap = 16;

	private const int kExpandBatchMin = 4;

	private int monsterSkillId;

	public void Init(int _monsterSkillId, Transform poolRoot)
	{
		monsterSkillId = _monsterSkillId;
		m_poolRoot = poolRoot;
		m_pool = new LinkedList<zbMonsterEffect>();
		m_activeMonsters = new LinkedList<zbMonsterEffect>();
	}

	public void Free()
	{
		m_pool.Clear();
		m_activeMonsters.Clear();
	}

	public zbMonsterEffect GetMonsterSkillEffect()
	{
		zbMonsterEffect zbMonsterEffect2 = null;
		if (m_pool.Count == 0)
		{
			ExpandPool(CalcExpandBatch());
		}
		if (m_pool.Count > 0)
		{
			zbMonsterEffect2 = m_pool.First.Value;
			m_pool.RemoveFirst();
			if (!((Component)zbMonsterEffect2).gameObject.activeSelf)
			{
				((Component)zbMonsterEffect2).gameObject.SetActive(true);
			}
			m_activeMonsters.AddLast(zbMonsterEffect2);
			return zbMonsterEffect2;
		}
		return null;
	}

	public void ReturnMonsterSkillEffect(zbMonsterEffect monster)
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
		foreach (zbMonsterEffect activeMonster in m_activeMonsters)
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
		GameObject val = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/combat/monsterEffect.prefab");
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			GameObject val2 = Object.Instantiate<GameObject>(val, m_poolRoot);
			zbMonsterEffect component = val2.GetComponent<zbMonsterEffect>();
			if ((Object)(object)component == (Object)null)
			{
				break;
			}
			val2.SetActive(false);
			m_pool.AddLast(component);
		}
	}
}
