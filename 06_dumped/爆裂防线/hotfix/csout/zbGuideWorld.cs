using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbGuideWorld : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_City);
		WorldMapGridDetail worldMapGridDetail = zbSelf.Instance.worldHandler.GetWorldMapGridDetail(1, -2);
		if (worldMapGridDetail != null && worldMapGridDetail.State == EWorldMapGridState.Explored)
		{
			ContinueGuide10();
			return;
		}
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbMainLevelWnd2.m_tabButtons[1], ContinueGuide0);
	}

	public void ContinueGuide0()
	{
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbMainLevelWnd2.SelectTab(1);
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbMainLevelWnd2.m_moduleChallenge.m_btnWorld, ContinueGuide0_1);
	}

	public void ContinueGuide0_1()
	{
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbMainLevelWnd2.m_moduleChallenge.OnWorld();
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbMainLevelWnd2.m_moduleChallenge.m_btnGoto, ContinueGuide0_2);
	}

	public void ContinueGuide0_2()
	{
		zbUIMgr.Instance.ShowWorld();
		zbUIMgr.Instance.ShowGuideWeak(10301, ContinueGuide1_1, 1100f);
	}

	public void ContinueGuide1_1()
	{
		zbWorldWnd zbWorldWnd2 = zbUIMgr.Instance.GetWndById(EWndId.World) as zbWorldWnd;
		zbUIMgr.Instance.ShowGuideWeak(10302, ContinueGuide1, 1100f, zbWorldWnd2.m_map.worldItemMgr.itemGuide.m_spriteBg);
	}

	public void ContinueGuide1()
	{
		zbUIMgr.Instance.GetWndById(EWndId.World);
		WorldMapGridDetail worldMapGridDetail = zbSelf.Instance.worldHandler.GetWorldMapGridDetail(1, -2);
		if (worldMapGridDetail != null)
		{
			zbUIMgr.Instance.ShowWorldReconExplore(worldMapGridDetail);
			zbUIMgr.Instance.ShowGuideWeak(10303, ContinueGuide2, 1200f);
		}
		else
		{
			ContinueGuide10();
		}
	}

	public void ContinueGuide2()
	{
		zbWorldReconExploreWnd zbWorldReconExploreWnd2 = zbUIMgr.Instance.GetWndById(EWndId.WorldReconExplore) as zbWorldReconExploreWnd;
		zbUIMgr.Instance.ShowGuideWeak(10304, ContinueGuide10, 1200f);
		zbUIMgr.Instance.ShowGuideFinger((VisualElement)(object)zbWorldReconExploreWnd2.m_btnConfirm);
	}

	public void ContinueGuide10()
	{
		zbGuide.Instance.StopCurGuideStep();
		zbUIMgr.Instance.GetWndById(EWndId.World);
	}
}
