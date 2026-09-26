using UnityEngine;

public class zbWallMgr
{
	public zbWall leftWall;

	public void Init(GameObject objScene)
	{
		leftWall = ((Component)((Component)objScene.transform.Find("homeMap")).transform.Find("front").Find("bg_wall_left")).GetComponent<zbWall>();
	}

	public void Build()
	{
		Refresh();
	}

	public void Free()
	{
	}

	public void Refresh()
	{
		leftWall.Refresh();
	}
}
