using UnityEngine.UIElements;

public class zbGuideWall2 : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbUIMgr.Instance.ShowGuideWeak(11521, ContinueGuide0);
	}

	public void ContinueGuide0()
	{
		zbWallWnd zbWallWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Wall) as zbWallWnd;
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbWallWnd2.m_tabButtons[1], ContinueGuide1);
	}

	public void ContinueGuide1()
	{
		(zbUIMgr.Instance.GetWndById(EWndId.Wall) as zbWallWnd).SelectTab(1);
		zbUIMgr.Instance.GetWndById(EWndId.WorldBossRule);
		zbUIMgr.Instance.ShowGuideWeak(11522, ContinueGuide2);
	}

	public void ContinueGuide2()
	{
		zbGuide.Instance.StopCurGuideStep();
	}
}
