using Proto.Enums;
using UnityEngine.UIElements;

public class zbGuideTreasure2 : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbUIMgr.Instance.ShowGuideWeak(12011, ContinueGuide0);
	}

	public void ContinueGuide0()
	{
		zbTreasureWnd zbTreasureWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Treasure) as zbTreasureWnd;
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbTreasureWnd2.m_tabButtons[1], ContinueGuide1);
	}

	public void ContinueGuide1()
	{
		(zbUIMgr.Instance.GetWndById(EWndId.Treasure) as zbTreasureWnd).SelectTab(1);
		zbUIMgr.Instance.ShowGuideWeak(12012, ContinueGuide2);
	}

	public void ContinueGuide2()
	{
		zbUIMgr.Instance.GetWndById(EWndId.Treasure);
		zbUIMgr.Instance.ShowGuideWeak(12013, ContinueGuide3);
	}

	public void ContinueGuide3()
	{
		zbGuide.Instance.StopCurGuideStep();
		if (zbSelf.Instance.barHandler.GetBarDetail(EBarType.Treasure).DailyFree == EBoolState.True)
		{
			zbTreasureWnd zbTreasureWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Treasure) as zbTreasureWnd;
			zbUIMgr.Instance.ShowGuideFinger((VisualElement)(object)zbTreasureWnd2.m_moduleBar.m_btnTreasure);
		}
	}
}
