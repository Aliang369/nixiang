using UnityEngine.UIElements;

public class zbGuideTreasure1 : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbUIMgr.Instance.ShowGuideWeak(12001, ContinueGuide0, 740f, (VisualElement)(object)zbMainLevelWnd2.m_MainBottom.m_btnHome);
		zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_Treasure_1);
	}

	public void ContinueGuide0()
	{
		zbHomeWnd zbHomeWnd2 = zbUIMgr.Instance.ShowHome() as zbHomeWnd;
		zbUIMgr.Instance.ShowGuideFinger((VisualElement)(object)zbHomeWnd2.m_btnTreasure);
		zbGuide.Instance.StopCurGuideStep();
	}
}
