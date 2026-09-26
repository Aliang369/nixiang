using UnityEngine.UIElements;

public class zbGuideCrossLadder1 : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbUIMgr.Instance.ShowGuideWeak(11811, ContinueGuide0, 740f, (VisualElement)(object)zbMainLevelWnd2.m_MainBottom.m_btnArena);
		zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_CrossLadder_1);
	}

	public void ContinueGuide0()
	{
		zbArenaMainWnd zbArenaMainWnd2 = zbUIMgr.Instance.ShowArenaMain() as zbArenaMainWnd;
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbArenaMainWnd2.m_lbCrossLadder, ContinueGuide1);
	}

	public void ContinueGuide1()
	{
		zbLinkHelper.OpenCrossLadder();
		zbGuide.Instance.StopCurGuideStep();
	}
}
