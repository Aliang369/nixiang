using System.Collections.Generic;
using UnityEngine;

public class zbZombiePool
{
	private LinkedList<zbZombie> m_pool;

	private LinkedList<zbZombie> m_activeZombies;

	private GameObject m_rootZombie;

	public void Init(GameObject objScene)
	{
		m_pool = new LinkedList<zbZombie>();
		m_activeZombies = new LinkedList<zbZombie>();
		m_rootZombie = ((Component)((Component)objScene.transform.Find("homeMap")).transform.Find("front").Find("root_idle_zombie")).gameObject;
	}

	public zbZombie Spawn()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		zbZombie zbZombie2 = null;
		if (m_pool.Count == 0)
		{
			ExpandPool();
		}
		if (m_pool.Count > 0)
		{
			zbZombie2 = m_pool.First.Value;
			m_pool.RemoveFirst();
			if (!((Component)zbZombie2).gameObject.activeSelf)
			{
				((Component)zbZombie2).gameObject.SetActive(true);
			}
			m_activeZombies.AddLast(zbZombie2);
			((Component)zbZombie2).gameObject.transform.SetParent(m_rootZombie.transform);
			((Component)zbZombie2).gameObject.transform.localPosition = Vector3.zero;
			return zbZombie2;
		}
		return null;
	}

	public void Recycle(zbZombie zombie)
	{
		if (((Component)zombie).gameObject.activeSelf)
		{
			((Component)zombie).gameObject.SetActive(false);
			m_activeZombies.Remove(zombie);
			m_pool.AddLast(zombie);
		}
	}

	private void ExpandPool()
	{
		zbZombie component = Object.Instantiate<GameObject>(zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/home/idle/zombie.prefab")).GetComponent<zbZombie>();
		m_pool.AddLast(component);
	}
}
