using Proto.Enums;
using UnityEngine.UIElements;

public class zbGuideElementTrial : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbMainLevelWnd2.m_MainBottom.m_btnChallenge, ContinueGuide0);
	}

	public void ContinueGuide0()
	{
		(zbUIMgr.Instance.ShowChallenge() as zbChallengeWnd).ActionDelayScrollTo(EFuncType.ElementTrial, ContinueGuide0_1);
	}

	public void ContinueGuide0_1()
	{
		zbChallengeWnd zbChallengeWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Challenge) as zbChallengeWnd;
		int tabItemIndex = zbChallengeWnd2.GetTabItemIndex(EFuncType.ElementTrial);
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbChallengeWnd2.m_items[tabItemIndex].m_btnFight, ContinueGuide1);
	}

	public void ContinueGuide1()
	{
		zbUIMgr.Instance.HideWnd(EWndId.Challenge);
		zbUIMgr.Instance.ShowElementTrial();
		zbUIMgr.Instance.ShowGuideWeak(10701, ContinueGuide2);
	}

	public void ContinueGuide2()
	{
		zbUIMgr.Instance.ShowGuideWeak(10702, ContinueGuide3);
	}

	public void ContinueGuide3()
	{
		zbGuide.Instance.StopCurGuideStep();
	}
}
