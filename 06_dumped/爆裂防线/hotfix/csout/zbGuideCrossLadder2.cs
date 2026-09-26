using UnityEngine.UIElements;

public class zbGuideCrossLadder2 : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbCrossLadderWnd zbCrossLadderWnd2 = zbUIMgr.Instance.GetWndById(EWndId.CrossLadder) as zbCrossLadderWnd;
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbCrossLadderWnd2.m_btnSettleAward, ContinueGuide2);
	}

	public void ContinueGuide2()
	{
		zbCrossLadderSettleAwardWnd zbCrossLadderSettleAwardWnd2 = zbUIMgr.Instance.ShowCrossLadderSettleAward() as zbCrossLadderSettleAwardWnd;
		zbUIMgr.Instance.ShowGuideWeak(11821, ContinueGuide3, 740f, (VisualElement)(object)zbCrossLadderSettleAwardWnd2.m_btnClose);
	}

	public void ContinueGuide3()
	{
		(zbUIMgr.Instance.GetWndById(EWndId.CrossLadderSettleAward) as zbCrossLadderSettleAwardWnd).Hide();
		zbCrossLadderWnd zbCrossLadderWnd2 = zbUIMgr.Instance.GetWndById(EWndId.CrossLadder) as zbCrossLadderWnd;
		zbUIMgr.Instance.ShowGuideWeak(11822, ContinueGuide4, 740f, (VisualElement)(object)zbCrossLadderWnd2.m_btnRule);
	}

	public void ContinueGuide4()
	{
		(zbUIMgr.Instance.GetWndById(EWndId.CrossLadder) as zbCrossLadderWnd).OnRule();
		zbRuleWnd zbRuleWnd2 = zbUIMgr.Instance.GetWndById(EWndId.WorldBossRule) as zbRuleWnd;
		zbUIMgr.Instance.ShowGuideWeak(11823, ContinueGuide5, 740f, (VisualElement)(object)zbRuleWnd2.m_btnClose);
	}

	public void ContinueGuide5()
	{
		(zbUIMgr.Instance.GetWndById(EWndId.WorldBossRule) as zbRuleWnd).Hide();
		zbUIMgr.Instance.ShowGuideWeak(11824, ContinueGuide6);
	}

	public void ContinueGuide6()
	{
		zbGuide.Instance.StopCurGuideStep();
	}
}
