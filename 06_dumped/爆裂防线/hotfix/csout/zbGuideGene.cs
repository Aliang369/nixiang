using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbGuideGene : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbUIMgr.Instance.ShowGuideWeak(11711, ContinueGuide0, 740f, (VisualElement)(object)zbMainLevelWnd2.m_MainBottom.m_btnFormation);
		zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_Gene);
	}

	public void ContinueGuide0()
	{
		zbFormationWnd zbFormationWnd2 = zbUIMgr.Instance.ShowFormation() as zbFormationWnd;
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbFormationWnd2.m_btnUpgrade, ContinueGuide1);
	}

	public void ContinueGuide1()
	{
		WarRoomDetail warRoomDetailBySkillType = zbSelf.Instance.heroHandler.GetWarRoomDetailBySkillType(ESkillType.Qiang);
		if (warRoomDetailBySkillType != null && warRoomDetailBySkillType.HeroTid > 0)
		{
			zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(warRoomDetailBySkillType.HeroTid);
			if (recordByKey != null)
			{
				zbHeroWnd zbHeroWnd2 = zbUIMgr.Instance.ShowHero(recordByKey) as zbHeroWnd;
				zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbHeroWnd2.m_tabButtons[2], ContinueGuide2);
			}
		}
	}

	public void ContinueGuide2()
	{
		zbHeroWnd zbHeroWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Hero) as zbHeroWnd;
		zbHeroWnd2.SelectTab(2);
		zbUIMgr.Instance.ShowGuideWeak(11712, ContinueGuide3, 740f, (VisualElement)(object)zbHeroWnd2.m_moduleGene.m_btnGeneList);
	}

	public void ContinueGuide3()
	{
		zbGeneWnd zbGeneWnd2 = zbUIMgr.Instance.ShowGene() as zbGeneWnd;
		zbUIMgr.Instance.ShowGuideWeak(11713, ContinueGuide4, 740f, (VisualElement)(object)zbGeneWnd2.m_btnClose);
	}

	public void ContinueGuide4()
	{
		(zbUIMgr.Instance.GetWndById(EWndId.Gene) as zbGeneWnd).Hide();
		zbUIMgr.Instance.GetWndById(EWndId.Hero);
		zbUIMgr.Instance.ShowGuideWeak(11714, ContinueGuide5);
	}

	public void ContinueGuide5()
	{
		zbGuide.Instance.StopCurGuideStep();
	}
}
