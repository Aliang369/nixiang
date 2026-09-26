using UnityEngine.UIElements;

public class zbGuideWall1 : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbMainLevelWnd2.m_MainBottom.m_btnHome, ContinueGuide0);
		zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_Wall_1);
	}

	public void ContinueGuide0()
	{
		zbHomeWnd zbHomeWnd2 = zbUIMgr.Instance.ShowHome() as zbHomeWnd;
		zbUIMgr.Instance.ShowGuideFinger((VisualElement)(object)zbHomeWnd2.m_btnWall);
		zbGuide.Instance.StopCurGuideStep();
	}
}
