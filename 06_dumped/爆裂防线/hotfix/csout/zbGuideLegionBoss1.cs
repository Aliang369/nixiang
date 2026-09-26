using System.Collections;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbGuideLegionBoss1 : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbMainLevelWnd2.m_MainBottom.m_btnChallenge, ContinueGuide0);
		zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_LegionBoss_1);
	}

	public void ContinueGuide0()
	{
		((MonoBehaviour)(zbUIMgr.Instance.ShowChallenge() as zbChallengeWnd)).StartCoroutine(ActionCoroutine());
	}

	public IEnumerator ActionCoroutine()
	{
		yield return null;
		(zbUIMgr.Instance.GetWndById(EWndId.Challenge) as zbChallengeWnd).ActionDelayScrollTo(EFuncType.LegionBoss, ContinueGuide0_1);
	}

	public void ContinueGuide0_1()
	{
		zbChallengeWnd zbChallengeWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Challenge) as zbChallengeWnd;
		int tabItemIndex = zbChallengeWnd2.GetTabItemIndex(EFuncType.LegionBoss);
		zbUIMgr.Instance.ShowGuideWeak(11011, ContinueGuide2, 740f, (VisualElement)(object)zbChallengeWnd2.m_items[tabItemIndex].m_btnFight);
	}

	public void ContinueGuide2()
	{
		zbGuide.Instance.StopCurGuideStep();
		zbChallengeWnd zbChallengeWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Challenge) as zbChallengeWnd;
		zbChallengeWnd2.m_items[zbChallengeWnd2.GetTabItemIndex(EFuncType.LegionBoss)].OnFight(null);
	}
}
