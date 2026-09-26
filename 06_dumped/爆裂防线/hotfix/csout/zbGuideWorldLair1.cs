using System.Collections;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbGuideWorldLair1 : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbMainLevelWnd2.m_MainBottom.m_btnChallenge, ContinueGuide0);
		zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_WorldLair_1);
	}

	public void ContinueGuide0()
	{
		((MonoBehaviour)(zbUIMgr.Instance.ShowChallenge() as zbChallengeWnd)).StartCoroutine(ActionCoroutine());
	}

	public IEnumerator ActionCoroutine()
	{
		yield return null;
		(zbUIMgr.Instance.GetWndById(EWndId.Challenge) as zbChallengeWnd).ActionDelayScrollTo(EFuncType.WorldLair, ContinueGuide2);
	}

	public void ContinueGuide2()
	{
		zbUIMgr.Instance.HideWnd(EWndId.WorldLairList);
		zbChallengeWnd zbChallengeWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Challenge) as zbChallengeWnd;
		zbUIMgr.Instance.ShowGuideWeak(10813, ContinueGuide3, 740f, (VisualElement)(object)zbChallengeWnd2.m_items[zbChallengeWnd2.GetTabItemIndex(EFuncType.WorldLair)].m_btnFight);
	}

	public void ContinueGuide3()
	{
		zbWsMsgHandler.Instance.WorldLair_Data_Req();
		zbGuide.Instance.StopCurGuideStep();
	}
}
