using System.Collections.Generic;

public class zbModIndex1026 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stMainHitBuffToSubSkill item = new stMainHitBuffToSubSkill
		{
			srcBuffId = (int)modRecord.clientEff[0],
			roll0to1 = modRecord.clientEff[1],
			subCount = (int)modRecord.clientEff[2],
			subSkillId = (int)modRecord.clientEff[3],
			skillType = modRecord.skillType[0]
		};
		if (!team.mainHitBuffToSubSkillBySkill.TryGetValue(item.skillType, out var value))
		{
			value = new List<stMainHitBuffToSubSkill>(2);
			team.mainHitBuffToSubSkillBySkill.Add(item.skillType, value);
		}
		value.Add(item);
	}
}
