using UnityEngine.UIElements;

public class zbGuideLegendary2 : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbUIMgr.Instance.ShowGuideWeak(11421, ContinueGuide0);
	}

	public void ContinueGuide0()
	{
		zbLegendaryWnd zbLegendaryWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Legendary) as zbLegendaryWnd;
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbLegendaryWnd2.m_btnZhanLing, ContinueGuide1);
	}

	public void ContinueGuide1()
	{
		zbUIMgr.Instance.ShowLegendaryZhanLing();
		zbUIMgr.Instance.ShowGuideWeak(11422, ContinueGuide2);
	}

	public void ContinueGuide2()
	{
		zbLegendaryZhanLingWnd zbLegendaryZhanLingWnd2 = zbUIMgr.Instance.GetWndById(EWndId.LegendaryZhanLing) as zbLegendaryZhanLingWnd;
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbLegendaryZhanLingWnd2.m_btnClose, ContinueGuide3);
	}

	public void ContinueGuide3()
	{
		zbUIMgr.Instance.HideWnd(EWndId.LegendaryZhanLing);
		zbLegendaryWnd zbLegendaryWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Legendary) as zbLegendaryWnd;
		zbUIMgr.Instance.ShowGuideWeak(11423, ContinueGuide4, 740f, zbLegendaryWnd2.ve_joinplayGuide);
	}

	public void ContinueGuide4()
	{
		zbGuide.Instance.StopCurGuideStep();
	}
}
