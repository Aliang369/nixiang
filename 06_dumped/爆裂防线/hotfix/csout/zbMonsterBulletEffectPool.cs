using System.Collections.Generic;
using UnityEngine;

public class zbMonsterBulletEffectPool
{
	private LinkedList<zbMonsterBullet> m_pool;

	private LinkedList<zbMonsterBullet> m_activeMonsters;

	private Transform m_poolRoot;

	private const int kExpandBatchCap = 16;

	private const int kExpandBatchMin = 4;

	public void Init(int monsterBulletId, Transform poolRoot)
	{
		m_poolRoot = poolRoot;
		m_pool = new LinkedList<zbMonsterBullet>();
		m_activeMonsters = new LinkedList<zbMonsterBullet>();
	}

	public void Free()
	{
		m_pool.Clear();
		m_activeMonsters.Clear();
	}

	public zbMonsterBullet GetMonsterBulletEffect()
	{
		zbMonsterBullet zbMonsterBullet2 = null;
		if (m_pool.Count == 0)
		{
			ExpandPool(CalcExpandBatch());
		}
		if (m_pool.Count > 0)
		{
			zbMonsterBullet2 = m_pool.First.Value;
			m_pool.RemoveFirst();
			if (!((Component)zbMonsterBullet2).gameObject.activeSelf)
			{
				((Component)zbMonsterBullet2).gameObject.SetActive(true);
			}
			m_activeMonsters.AddLast(zbMonsterBullet2);
			return zbMonsterBullet2;
		}
		return null;
	}

	public void ReturnMonsterBulletEffect(zbMonsterBullet monster)
	{
		if (((Component)monster).gameObject.activeSelf)
		{
			((Component)monster).gameObject.SetActive(false);
			m_activeMonsters.Remove(monster);
			m_pool.AddLast(monster);
		}
	}

	public void PreHeat(int count)
	{
		if (count > 0)
		{
			ExpandPool(count);
		}
	}

	public void CollectActiveSpineZ(List<float> results)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		foreach (zbMonsterBullet activeMonster in m_activeMonsters)
		{
			if (!((Object)(object)activeMonster == (Object)null) && !activeMonster.m_spineGpuTakenOver && !((Object)(object)activeMonster.animationSpine == (Object)null))
			{
				Transform transform = ((Component)activeMonster.animationSpine).transform;
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
		GameObject val = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/combat/monsterBullet.prefab");
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			GameObject val2 = Object.Instantiate<GameObject>(val, m_poolRoot);
			zbMonsterBullet component = val2.GetComponent<zbMonsterBullet>();
			if ((Object)(object)component == (Object)null)
			{
				break;
			}
			val2.SetActive(false);
			m_pool.AddLast(component);
		}
	}
}
