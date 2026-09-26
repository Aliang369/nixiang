using UnityEngine;
using UnityEngine.UIElements;

public class zbGuideArena1 : zbGuideBase
{
	private bool showPvpModSelect;

	public override void Start()
	{
		base.Start();
		zbGameSave.Instance.SetLong(zbSelf.Instance.arenaHandler.saveShowLegionWarEffectKey, zbSelf.Instance.timeHandler.GetServerTime());
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbUIMgr.Instance.ShowGuideWeak(11611, ContinueGuide0, 740f, (VisualElement)(object)zbMainLevelWnd2.m_MainBottom.m_btnArena);
		zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_Arena_1);
	}

	public void ContinueGuide0()
	{
		zbArenaMainWnd zbArenaMainWnd2 = zbUIMgr.Instance.ShowArenaMain() as zbArenaMainWnd;
		zbUIMgr.Instance.ShowGuideWeak(11612, ContinueGuide1, 740f, (VisualElement)(object)zbArenaMainWnd2.m_bottomPanel.m_btnEdit);
	}

	public void ContinueGuide1()
	{
		zbWsMsgHandler.Instance.PvpSet_ModStart_Req();
		showPvpModSelect = true;
	}

	public void ContinueGuide2()
	{
		zbUIMgr.Instance.GetWndById(EWndId.PvpModSelect);
		zbUIMgr.Instance.ShowGuideWeak(11614, ContinueGuide10);
	}

	public void ContinueGuide10()
	{
		zbUIMgr.Instance.HideWnd(EWndId.Guide);
		zbUIMgr.Instance.HideWnd(EWndId.GuideWeak);
	}

	public override void Update()
	{
		base.Update();
		if (showPvpModSelect)
		{
			zbPvpModSelectWnd zbPvpModSelectWnd2 = zbUIMgr.Instance.GetWndById(EWndId.PvpModSelect) as zbPvpModSelectWnd;
			if ((Object)(object)zbPvpModSelectWnd2 != (Object)null && zbPvpModSelectWnd2.IsShow())
			{
				zbUIMgr.Instance.ShowGuideWeak(11613, ContinueGuide2);
				showPvpModSelect = false;
			}
		}
	}
}
