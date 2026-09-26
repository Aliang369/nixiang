using Proto.Types;
using UnityEngine;

public class zbMineWarMgr
{
	private static zbMineWarMgr sInstance;

	public zbMineWarAwardHud mineWarAwardHud;

	public zbMineWarPoint[] mineWarPointList { get; set; } = new zbMineWarPoint[49];

	public static zbMineWarMgr Instance
	{
		get
		{
			if (sInstance == null)
			{
				sInstance = new zbMineWarMgr();
			}
			return sInstance;
		}
	}

	public void Init(GameObject objScene)
	{
		Transform val = objScene.transform.Find("front");
		for (int i = 0; i < 49; i++)
		{
			zbMineWarPoint component = ((Component)val.Find("mineWarPoint" + (i + 1))).GetComponent<zbMineWarPoint>();
			mineWarPointList[i] = component;
			component.Init(i + 1);
		}
		mineWarAwardHud = new zbMineWarAwardHud();
		mineWarAwardHud.Init();
	}

	public void Free()
	{
	}

	public void Refresh()
	{
		bool flag = false;
		foreach (MineWarPointDetail point in zbSelf.Instance.mineWarHandler.mineWarMapInfo.PointList)
		{
			mineWarPointList[point.PointId - 1].Refresh(point);
			if (point.PlayerId == zbSelf.Instance.GetRoleId())
			{
				mineWarAwardHud.Refresh(point, mineWarPointList[point.PointId - 1]);
				flag = true;
			}
		}
		if (!flag)
		{
			mineWarAwardHud.Refresh(null, null);
		}
	}

	public void Update()
	{
		zbMineWarPoint[] array = mineWarPointList;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].mineWarHud.Update();
		}
		mineWarAwardHud.Update();
	}
}
