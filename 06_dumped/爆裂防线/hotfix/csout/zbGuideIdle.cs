using UnityEngine.UIElements;

public class zbGuideIdle : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbMainLevelWnd2.m_btnSweep, OpenWallIdleWnd);
	}

	public void OpenWallIdleWnd()
	{
		zbUIMgr.Instance.ShowWallIdle();
		zbUIMgr.Instance.ShowGuideWeak(10101, ContinueGuide1, 380f);
	}

	public void ContinueGuide1()
	{
		zbUIMgr.Instance.GetWndById(EWndId.WallIdle);
		zbUIMgr.Instance.ShowGuideWeak(10103, ContinueGuide2, 380f);
	}

	public void ContinueGuide2()
	{
		zbWallIdleWnd zbWallIdleWnd2 = zbUIMgr.Instance.GetWndById(EWndId.WallIdle) as zbWallIdleWnd;
		zbUIMgr.Instance.ShowGuideWeak(10102, ContinueGuide3, 380f, (VisualElement)(object)zbWallIdleWnd2.m_btnGet);
	}

	public void ContinueGuide3()
	{
		zbWallIdleWnd zbWallIdleWnd2 = zbUIMgr.Instance.GetWndById(EWndId.WallIdle) as zbWallIdleWnd;
		zbUIMgr.Instance.ShowGuideFinger((VisualElement)(object)zbWallIdleWnd2.m_btnGet);
		zbGuide.Instance.StopCurGuideStep();
	}
}
