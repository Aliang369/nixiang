using UnityEngine.UIElements;

public class zbGuideHome1 : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbMainLevelWnd2.m_MainBottom.m_btnHome, ContinueGuide0);
		zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_Home1);
	}

	public void ContinueGuide0()
	{
		zbUIMgr.Instance.ShowHome();
		zbGuide.Instance.StopCurGuideStep();
	}
}
