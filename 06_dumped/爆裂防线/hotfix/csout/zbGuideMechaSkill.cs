using UnityEngine.UIElements;

public class zbGuideMechaSkill : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbUIMgr.Instance.ShowGuideWeak(11911, ContinueGuide0, 740f, (VisualElement)(object)zbMainLevelWnd2.m_MainBottom.m_btnFormation);
		zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_MechaSkill);
	}

	public void ContinueGuide0()
	{
		zbFormationWnd zbFormationWnd2 = zbUIMgr.Instance.ShowFormation() as zbFormationWnd;
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbFormationWnd2.m_btnMecha, ContinueGuide1);
	}

	public void ContinueGuide1()
	{
		if (zbSelf.Instance.GetMechaId() <= 0)
		{
			ContinueGuide3();
			return;
		}
		int mechaId = zbSelf.Instance.GetMechaId();
		zbMechaCsvRecord mechaRecord = zbSelf.Instance.mechaHandler.GetMechaRecord(mechaId);
		if (mechaRecord != null)
		{
			zbMechaWnd zbMechaWnd2 = zbUIMgr.Instance.ShowMecha(mechaRecord) as zbMechaWnd;
			zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbMechaWnd2.m_tabButtons[2], ContinueGuide2);
		}
		else
		{
			zbGuide.Instance.StopCurGuideStep();
		}
	}

	public void ContinueGuide2()
	{
		(zbUIMgr.Instance.GetWndById(EWndId.Mecha) as zbMechaWnd).SelectTab(2);
		zbUIMgr.Instance.ShowGuideWeak(11912, ContinueGuide3);
	}

	public void ContinueGuide3()
	{
		zbGuide.Instance.StopCurGuideStep();
	}
}
