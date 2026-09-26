using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbGuideHeroAwaken : zbGuideBase
{
	public override void Start()
	{
		base.Start();
		zbMainLevelWnd zbMainLevelWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MainLevel) as zbMainLevelWnd;
		zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbMainLevelWnd2.m_MainBottom.m_btnFormation, ContinueGuide0);
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
				zbUIMgr.Instance.ShowGuideWnd((VisualElement)(object)zbHeroWnd2.m_tabButtons[1], ContinueGuide2);
			}
		}
	}

	public void ContinueGuide2()
	{
		(zbUIMgr.Instance.GetWndById(EWndId.Hero) as zbHeroWnd).SelectTab(1);
		zbUIMgr.Instance.ShowGuideWeak(10911, ContinueGuide3);
	}

	public void ContinueGuide3()
	{
		zbHeroWnd zbHeroWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Hero) as zbHeroWnd;
		zbUIMgr.Instance.ShowGuideFinger((VisualElement)(object)zbHeroWnd2.m_moduleAwaken.m_btnUpgrade);
		zbGuide.Instance.StopCurGuideStep();
	}
}
