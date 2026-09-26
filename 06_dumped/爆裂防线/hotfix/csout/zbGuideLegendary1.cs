using UnityEngine.UIElements;

public class zbGuideLegendary1 : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbMainLevelWnd2.m_tabButtons[1], ContinueGuide0);
		zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_Legendary_1);
	}

	public void ContinueGuide0()
	{
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbMainLevelWnd2.SelectTab(1);
		zbUIMgr.Instance.ShowGuideWeak(11411, ContinueGuide0_1, 1100f, (VisualElement)(object)zbMainLevelWnd2.m_moduleChallenge.m_btnLegendary);
	}

	public void ContinueGuide0_1()
	{
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbMainLevelWnd2.m_moduleChallenge.OnLegendary();
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbMainLevelWnd2.m_moduleChallenge.m_btnGoto, ContinueGuide1);
	}

	public void ContinueGuide1()
	{
		zbGuide.Instance.StopCurGuideStep();
		zbLinkHelper.OpenLegendary();
	}
}
