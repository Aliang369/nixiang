using UnityEngine.UIElements;

public class zbGuideElite : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_Elite);
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbMainLevelWnd2.m_tabButtons[1], ContinueGuide0);
	}

	public void ContinueGuide0()
	{
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbMainLevelWnd2.SelectTab(1);
		zbUIMgr.Instance.ShowGuideWeak(10501, ContinueGuide0_1, 1100f, (VisualElement)(object)zbMainLevelWnd2.m_moduleChallenge.m_btnElite);
	}

	public void ContinueGuide0_1()
	{
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbMainLevelWnd2.m_moduleChallenge.OnElite();
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbMainLevelWnd2.m_moduleChallenge.m_btnGoto, ContinueGuide1);
	}

	public void ContinueGuide1()
	{
		zbEliteQuestWnd zbEliteQuestWnd2 = zbUIMgr.Instance.ShowEliteQuest() as zbEliteQuestWnd;
		zbUIMgr.Instance.ShowGuideWeak(10502, ContinueGuide2, 1100f, zbEliteQuestWnd2.ve_joinplayGuide);
	}

	public void ContinueGuide2()
	{
		zbUIMgr.Instance.ShowMainQuestJoin();
		zbUIMgr.Instance.ShowGuideWeak(10503, ContinueGuide3);
	}

	public void ContinueGuide3()
	{
		zbGuide.Instance.StopCurGuideStep();
	}
}
