using System.Collections.Generic;

public class zbModIndex1001 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stMainHitToSubSkill item = new stMainHitToSubSkill
		{
			damageType = (int)modRecord.clientEff[0],
			roll0to1 = modRecord.clientEff[1],
			subCount = (int)modRecord.clientEff[2],
			subSkillId = (int)modRecord.clientEff[3],
			skillType = modRecord.skillType[0]
		};
		if (!team.mainHitToSubSkillBySkill.TryGetValue(item.skillType, out var value))
		{
			value = new List<stMainHitToSubSkill>(2);
			team.mainHitToSubSkillBySkill.Add(item.skillType, value);
		}
		value.Add(item);
	}
}
