using System.Collections.Generic;
using UnityEngine;

public class zbPaotaiBulletPool
{
	private LinkedList<zbPaotaiBullet> m_pool;

	private LinkedList<zbPaotaiBullet> m_activeBullets;

	private GameObject m_rootBullet;

	public void Init(GameObject objScene)
	{
		m_pool = new LinkedList<zbPaotaiBullet>();
		m_activeBullets = new LinkedList<zbPaotaiBullet>();
		m_rootBullet = ((Component)((Component)objScene.transform.Find("homeMap")).transform.Find("front").Find("root_idle_paotai_bullet")).gameObject;
	}

	public zbPaotaiBullet Spawn()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		zbPaotaiBullet zbPaotaiBullet2 = null;
		if (m_pool.Count == 0)
		{
			ExpandPool();
		}
		if (m_pool.Count > 0)
		{
			zbPaotaiBullet2 = m_pool.First.Value;
			m_pool.RemoveFirst();
			if (!((Component)zbPaotaiBullet2).gameObject.activeSelf)
			{
				((Component)zbPaotaiBullet2).gameObject.SetActive(true);
			}
			m_activeBullets.AddLast(zbPaotaiBullet2);
			((Component)zbPaotaiBullet2).gameObject.transform.SetParent(m_rootBullet.transform);
			((Component)zbPaotaiBullet2).gameObject.transform.localPosition = Vector3.zero;
			return zbPaotaiBullet2;
		}
		return null;
	}

	public void Recycle(zbPaotaiBullet bullet)
	{
		if (((Component)bullet).gameObject.activeSelf)
		{
			((Component)bullet).gameObject.SetActive(false);
			m_activeBullets.Remove(bullet);
			m_pool.AddLast(bullet);
		}
	}

	private void ExpandPool()
	{
		zbPaotaiBullet component = Object.Instantiate<GameObject>(zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/home/idle/paotaiBullet.prefab")).GetComponent<zbPaotaiBullet>();
		m_pool.AddLast(component);
	}
}
