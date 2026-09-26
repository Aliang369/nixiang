using System.Collections.Generic;

public class zbModIndex1015 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stMainFirstHitToSubSkill item = new stMainFirstHitToSubSkill
		{
			roll0to1 = modRecord.clientEff[0],
			subCount = (int)modRecord.clientEff[1],
			subSkillId = (int)modRecord.clientEff[2],
			skillType = modRecord.skillType[0]
		};
		if (!team.mainFirstHitToSubSkillBySkill.TryGetValue(item.skillType, out var value))
		{
			value = new List<stMainFirstHitToSubSkill>(2);
			team.mainFirstHitToSubSkillBySkill.Add(item.skillType, value);
		}
		value.Add(item);
	}
}
