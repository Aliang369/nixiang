using UnityEngine.UIElements;

public class zbGuideEndlessHorde2 : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbUIMgr.Instance.ShowGuideWeak(11321, ContinueGuide0);
	}

	public void ContinueGuide0()
	{
		zbEndlessHordeWnd zbEndlessHordeWnd2 = zbUIMgr.Instance.GetWndById(EWndId.EndlessHorde) as zbEndlessHordeWnd;
		zbUIMgr.Instance.ShowGuideWeak(11322, ContinueGuide1, 740f, (VisualElement)(object)zbEndlessHordeWnd2.m_btnRule);
	}

	public void ContinueGuide1()
	{
		(zbUIMgr.Instance.GetWndById(EWndId.EndlessHorde) as zbEndlessHordeWnd).OnRule();
		zbRuleWnd zbRuleWnd2 = zbUIMgr.Instance.GetWndById(EWndId.WorldBossRule) as zbRuleWnd;
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbRuleWnd2.m_btnClose, ContinueGuide2);
	}

	public void ContinueGuide2()
	{
		zbUIMgr.Instance.HideWnd(EWndId.WorldBossRule);
		zbUIMgr.Instance.ShowGuideWeak(11323, ContinueGuide3);
	}

	public void ContinueGuide3()
	{
		zbGuide.Instance.StopCurGuideStep();
	}
}
