using System.Collections.Generic;
using UnityEngine;

public class zbMonsterDieFxPool
{
	private LinkedList<zbMonsterDieFx> m_pool;

	private LinkedList<zbMonsterDieFx> m_activeDieFxs;

	private Transform m_poolRoot;

	private const int kExpandBatchCap = 16;

	private const int kExpandBatchMin = 4;

	public void Init(Transform nodeRoot)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		m_pool = new LinkedList<zbMonsterDieFx>();
		m_activeDieFxs = new LinkedList<zbMonsterDieFx>();
		m_poolRoot = new GameObject("RootDieFxPool").transform;
		m_poolRoot.SetParent(nodeRoot, false);
	}

	public zbMonsterDieFx GetDieFx()
	{
		zbMonsterDieFx zbMonsterDieFx2 = null;
		if (m_pool.Count == 0)
		{
			ExpandPool(CalcExpandBatch());
		}
		if (m_pool.Count > 0)
		{
			zbMonsterDieFx2 = m_pool.First.Value;
			m_pool.RemoveFirst();
			if (!((Component)zbMonsterDieFx2).gameObject.activeSelf)
			{
				((Component)zbMonsterDieFx2).gameObject.SetActive(true);
			}
			m_activeDieFxs.AddLast(zbMonsterDieFx2);
			return zbMonsterDieFx2;
		}
		return null;
	}

	public void ReturnDieFx(zbMonsterDieFx dieFx)
	{
		if (((Component)dieFx).gameObject.activeSelf)
		{
			((Component)dieFx).gameObject.SetActive(false);
			m_activeDieFxs.Remove(dieFx);
			m_pool.AddLast(dieFx);
		}
	}

	public void Free()
	{
		m_pool.Clear();
		m_activeDieFxs.Clear();
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
		int num = (m_pool.Count + m_activeDieFxs.Count + 1) / 2;
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
		GameObject val = zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/combat/monsterDieFx.prefab");
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			GameObject val2 = Object.Instantiate<GameObject>(val, m_poolRoot);
			zbMonsterDieFx component = val2.GetComponent<zbMonsterDieFx>();
			if ((Object)(object)component == (Object)null)
			{
				break;
			}
			val2.SetActive(false);
			m_pool.AddLast(component);
		}
	}
}
