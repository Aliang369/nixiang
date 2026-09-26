using UnityEngine.UIElements;

public class zbGuideFriend : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbMainLevelWnd2.m_topBar.m_btnFriend, ContinueGuide0);
	}

	public void ContinueGuide0()
	{
		zbFriendWnd zbFriendWnd2 = zbUIMgr.Instance.ShowFriends() as zbFriendWnd;
		zbUIMgr.Instance.ShowGuideWeak(10401, ContinueGuide1, 1100f, (VisualElement)(object)zbFriendWnd2.m_tabButtons[1]);
	}

	public void ContinueGuide1()
	{
		(zbUIMgr.Instance.GetWndById(EWndId.Friends) as zbFriendWnd).SelectTab(1);
		zbUIMgr.Instance.ShowGuideWeak(10402, ContinueGuide2, 1100f);
	}

	public void ContinueGuide2()
	{
		zbFriendWnd zbFriendWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Friends) as zbFriendWnd;
		zbUIMgr.Instance.ShowGuideFinger((VisualElement)(object)zbFriendWnd2.m_panelAdd.m_btnAllApply);
		zbGuide.Instance.StopCurGuideStep();
	}
}
