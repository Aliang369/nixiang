using UnityEngine.UIElements;

public class zbGuideLegion : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbMainLevelWnd2.m_MainBottom.m_btnLegion, ContinueGuide0);
	}

	public void ContinueGuide0()
	{
		zbUIMgr.Instance.ShowLegionRecommend();
		zbUIMgr.Instance.ShowGuideWeak(10601, ContinueGuide1);
	}

	public void ContinueGuide1()
	{
		zbGuide.Instance.StopCurGuideStep();
	}
}
