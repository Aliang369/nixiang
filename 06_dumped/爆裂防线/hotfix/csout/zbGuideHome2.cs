using UnityEngine;
using UnityEngine.UIElements;

public class zbGuideHome2 : zbGuideBase
{
	public override void Start()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		base.Start();
		((Component)zbCameraHelper.Instance.mainCamera).transform.position = new Vector3(1.2f, -11f, ((Component)zbCameraHelper.Instance.mainCamera).transform.position.z);
		zbUIMgr.Instance.GetWndById(EWndId.Home);
		zbUIMgr.Instance.ShowGuideWeak(10201, ContinueGuide1);
	}

	public void ContinueGuide1()
	{
		zbUIMgr.Instance.GetWndById(EWndId.Home);
		zbRoom roomByXY = zbHomeMgr.Instance.roomMgr.GetRoomByXY(3, -1);
		if ((Object)(object)roomByXY == (Object)null)
		{
			if (!zbHomeMgr.Instance.IsBuildOk())
			{
				zbUIMgr.Instance.ShowGuideWeak(10201, ContinueGuide1);
			}
			else
			{
				ContinueGuide3();
			}
		}
		else
		{
			zbUIMgr.Instance.ShowGuideWeak(10202, ContinueGuide2, 380f, roomByXY.roomHud.m_Root);
		}
	}

	public void ContinueGuide2()
	{
		zbRoom roomByXY = zbHomeMgr.Instance.roomMgr.GetRoomByXY(3, -1);
		zbUIMgr.Instance.ShowRoomDetail(roomByXY.roomData);
		zbUIMgr.Instance.ShowGuideWeak(10203, ContinueGuide3, 380f);
	}

	public void ContinueGuide3()
	{
		zbGuide.Instance.StopCurGuideStep();
		zbRoomDetailWnd zbRoomDetailWnd2 = zbUIMgr.Instance.GetWndById(EWndId.RoomDetail) as zbRoomDetailWnd;
		if ((Object)(object)zbRoomDetailWnd2 != (Object)null)
		{
			zbUIMgr.Instance.ShowGuideFinger((VisualElement)(object)zbRoomDetailWnd2.m_btnUpgrade);
		}
	}
}
