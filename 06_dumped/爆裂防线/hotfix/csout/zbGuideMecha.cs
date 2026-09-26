using UnityEngine.UIElements;

public class zbGuideMecha : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbUIMgr.Instance.ShowGuideWeak(11211, ContinueGuide0, 740f, (VisualElement)(object)zbMainLevelWnd2.m_MainBottom.m_btnFormation);
		zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_Mecha);
	}

	public void ContinueGuide0()
	{
		zbFormationWnd zbFormationWnd2 = zbUIMgr.Instance.ShowFormation() as zbFormationWnd;
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbFormationWnd2.m_btnMecha, ContinueGuide1);
	}

	public void ContinueGuide1()
	{
		int mechaId = ((zbSelf.Instance.GetMechaId() > 0) ? zbSelf.Instance.GetMechaId() : 104);
		zbMechaCsvRecord mechaRecord = zbSelf.Instance.mechaHandler.GetMechaRecord(mechaId);
		if (mechaRecord != null)
		{
			zbUIMgr.Instance.ShowMecha(mechaRecord);
			zbUIMgr.Instance.ShowGuideWeak(11212, ContinueGuide2);
		}
		else
		{
			zbGuide.Instance.StopCurGuideStep();
		}
	}

	public void ContinueGuide2()
	{
		zbMechaWnd zbMechaWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Mecha) as zbMechaWnd;
		zbUIMgr.Instance.ShowGuideFinger((VisualElement)(object)zbMechaWnd2.m_lockPanel.m_btnActive);
		zbGuide.Instance.StopCurGuideStep();
	}
}
