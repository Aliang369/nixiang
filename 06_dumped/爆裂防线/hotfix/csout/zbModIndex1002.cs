using Proto.Enums;
using UnityEngine;

public class zbModIndex1002 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		zbDataConfig.Instance.skillCsvTable.GetRecordByKey((int)modRecord.clientEff[0]);
		zbHero heroBySkillType = team.GetHeroBySkillType((ESkillType)modRecord.skillType[0]);
		if ((Object)(object)heroBySkillType != (Object)null && (int)modRecord.clientEff[0] > 0)
		{
			heroBySkillType.RefreshRiseEffect((int)modRecord.clientEff[0]);
		}
	}
}
