using System.Collections.Generic;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;

public class zbFloorMgr
{
	public List<zbFloor> floors = new List<zbFloor>();

	private GameObject m_objScene;

	private Transform m_frontTransform;

	private Transform m_floorBlack;

	public void Init(GameObject objScene)
	{
		m_objScene = objScene;
		m_frontTransform = ((Component)m_objScene.transform.Find("homeMap")).transform.Find("front");
		m_floorBlack = m_frontTransform.Find("floor_black");
	}

	public void Build()
	{
		for (int num = 3; num >= -8; num--)
		{
			GameObject gameObject = ((Component)m_frontTransform.Find("floor" + num)).gameObject;
			if ((Object)(object)gameObject != (Object)null)
			{
				zbFloor component = gameObject.GetComponent<zbFloor>();
				if ((Object)(object)component != (Object)null)
				{
					component.Build();
					floors.Add(component);
				}
			}
		}
		RefreshFloorBlack();
	}

	public void Free()
	{
	}

	public void Refresh()
	{
		foreach (zbFloor floor in floors)
		{
			if ((Object)(object)floor != (Object)null)
			{
				floor.Refresh();
			}
		}
		RefreshFloorBlack();
	}

	private void RefreshFloorBlack()
	{
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		int num = zbSelf.Instance.homelandHandler.GetNextActiveFloor();
		if (num == 0)
		{
			RepeatedField<HomelandFloorDetail> floorList = zbServerData.sGameData.HomelandInfo.BaseInfo.FloorList;
			num = floorList[floorList.Count - 1].Floor;
		}
		float num2 = -0.3f;
		foreach (HomelandFloorDetail floor in zbServerData.sGameData.HomelandInfo.BaseInfo.FloorList)
		{
			if (floor.State == EActiveState.Active)
			{
				num = floor.Floor;
				break;
			}
		}
		((Component)m_floorBlack).transform.position = new Vector3(7.6f, (float)num * 2.2f + num2, 0f);
		((Component)m_floorBlack).gameObject.SetActive(num < 0);
	}

	public void CheakFloorUnlock()
	{
		foreach (zbFloor floor in floors)
		{
			if ((Object)(object)floor != (Object)null && floor.floorIndex == zbSelf.Instance.homelandHandler.newFloorUnlock)
			{
				zbSelf.Instance.homelandHandler.newFloorUnlock = -1;
				floor.PlayFloorUnlockAnimation();
				break;
			}
		}
	}

	public void UpdateHudWhenCameraMove()
	{
		foreach (zbFloor floor in floors)
		{
			floor.floorHud.Update();
		}
	}

	public void Update()
	{
	}
}
