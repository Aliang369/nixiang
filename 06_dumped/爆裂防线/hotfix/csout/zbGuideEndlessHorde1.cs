using System.Collections;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbGuideEndlessHorde1 : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbUIMgr.Instance.ShowGuideWeak(11311, ContinueGuide0, 740f, (VisualElement)(object)zbMainLevelWnd2.m_MainBottom.m_btnChallenge);
		zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_EndlessHorde_1);
	}

	public void ContinueGuide0()
	{
		((MonoBehaviour)(zbUIMgr.Instance.ShowChallenge() as zbChallengeWnd)).StartCoroutine(ActionCoroutine());
	}

	public IEnumerator ActionCoroutine()
	{
		yield return null;
		(zbUIMgr.Instance.GetWndById(EWndId.Challenge) as zbChallengeWnd).ActionDelayScrollTo(EFuncType.EndlessHorde, ContinueGuide0_1);
	}

	public void ContinueGuide0_1()
	{
		zbChallengeWnd zbChallengeWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Challenge) as zbChallengeWnd;
		int tabItemIndex = zbChallengeWnd2.GetTabItemIndex(EFuncType.EndlessHorde);
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbChallengeWnd2.m_items[tabItemIndex].m_btnFight, ContinueGuide2);
	}

	public void ContinueGuide2()
	{
		zbGuide.Instance.StopCurGuideStep();
		zbUIMgr.Instance.ShowEndLessHorde();
	}
}
