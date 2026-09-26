using Proto.Enums;
using UnityEngine;

public class zbModIndex1000 : zbModBase
{
	public override void RefreshProperty()
	{
		ESkillType skillType = (ESkillType)modRecord.clientEff[0];
		zbHero zbHero2 = team.CreateHero(skillType);
		base.RefreshProperty();
		if ((Object)(object)zbHero2 != (Object)null)
		{
			if (isUserSelect)
			{
				combatData.ActionChuchangAnim(zbHero2.GetAttrValueByType(EAttrSkillType.AlertRange));
			}
			zbHero2.team.combatStat.AddDamageStat(zbHero2.GetHeroTid(), null, 0L);
			if (combatData != null && combatData.combatRoute != null)
			{
				combatData.combatRoute.SetRouteWidth();
			}
		}
		if (zbSelf.Instance.guideHandler.IsFirstSelectHero(skillType))
		{
			zbUIMgr.Instance.ShowGuidePrinter(2001);
			zbSelf.Instance.guideHandler.SetFirstSelectHero(flag: false);
		}
	}
}
