using System;
using System.Collections.Generic;
using UnityEngine;

public class zbMonsterPool
{
	private int m_monsterTid;

	private LinkedList<zbMonster> m_pool;

	private LinkedList<zbMonster> m_activeMonsters;

	private Transform m_poolRoot;

	private const int kExpandBatchCap = 16;

	private const int kExpandBatchMin = 4;

	private const int kExpandPerFrame = 4;

	private int m_pendingExpand;

	private GameObject m_cachedPrefab;

	public int IdleCount => m_pool.Count;

	public void Init(int monsterTid, Transform poolRoot)
	{
		m_monsterTid = monsterTid;
		m_poolRoot = poolRoot;
		m_pool = new LinkedList<zbMonster>();
		m_activeMonsters = new LinkedList<zbMonster>();
	}

	public zbMonster GetMonster()
	{
		zbMonster zbMonster2 = null;
		if (m_pool.Count == 0)
		{
			int num = CalcExpandBatch();
			zbPerfLog.Inc("怪物池战中扩容数", num);
			ExpandPool(num);
			m_pendingExpand += num;
		}
		if (m_pool.Count > 0)
		{
			zbMonster2 = m_pool.First.Value;
			m_pool.RemoveFirst();
			if (!((Component)zbMonster2).gameObject.activeSelf)
			{
				((Component)zbMonster2).gameObject.SetActive(true);
			}
			m_activeMonsters.AddLast(zbMonster2);
			return zbMonster2;
		}
		return null;
	}

	public void ReturnMonster(zbMonster monster)
	{
		if (((Component)monster).gameObject.activeSelf)
		{
			((Component)monster).gameObject.SetActive(false);
			m_activeMonsters.Remove(monster);
			m_pool.AddLast(monster);
		}
	}

	public void Free()
	{
		m_pool.Clear();
		m_activeMonsters.Clear();
		m_pendingExpand = 0;
		m_cachedPrefab = null;
	}

	public void UpdateExpand()
	{
		if (m_pendingExpand > 0)
		{
			if (m_pool.Count >= m_pendingExpand)
			{
				m_pendingExpand = 0;
				return;
			}
			int num = Math.Min(m_pendingExpand, 4);
			ExpandPool(num);
			m_pendingExpand -= num;
			zbPerfLog.Inc("怪物池分帧扩容数", num);
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
		if ((Object)(object)m_cachedPrefab == (Object)null)
		{
			m_cachedPrefab = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/combat/monster.prefab");
		}
		GameObject cachedPrefab = m_cachedPrefab;
		if ((Object)(object)cachedPrefab == (Object)null)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			GameObject val = Object.Instantiate<GameObject>(cachedPrefab, m_poolRoot);
			zbMonster component = val.GetComponent<zbMonster>();
			if ((Object)(object)component == (Object)null)
			{
				break;
			}
			val.SetActive(false);
			m_pool.AddLast(component);
		}
	}
}
