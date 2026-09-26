using UnityEngine;

public class zbPaotaiMgr
{
	public zbPaotai paotai;

	public zbPaotaiBulletPool bulletPool;

	public void Init(GameObject objScene)
	{
		paotai = ((Component)((Component)objScene.transform.Find("homeMap")).transform.Find("front").Find("bg_wall_left").Find("wall_paotai")).GetComponent<zbPaotai>();
		bulletPool = new zbPaotaiBulletPool();
		bulletPool.Init(objScene);
	}

	public void Free()
	{
	}

	public void Build()
	{
		paotai.Refresh();
	}

	public void Refresh()
	{
		paotai.Refresh();
	}

	public zbPaotaiBullet Spawn()
	{
		return bulletPool.Spawn();
	}

	public void Recycle(zbPaotaiBullet bullet)
	{
		bulletPool.Recycle(bullet);
	}
}
