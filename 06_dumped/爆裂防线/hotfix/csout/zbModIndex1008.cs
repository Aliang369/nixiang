using System.Collections.Generic;

public class zbModIndex1008 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stMainEndToSubSkill item = new stMainEndToSubSkill
		{
			roll0to1 = modRecord.clientEff[0],
			subCount = (int)modRecord.clientEff[1],
			subSkillId = (int)modRecord.clientEff[2],
			skillType = modRecord.skillType[0]
		};
		if (!team.mainEndToSubSkillBySkill.TryGetValue(item.skillType, out var value))
		{
			value = new List<stMainEndToSubSkill>(2);
			team.mainEndToSubSkillBySkill.Add(item.skillType, value);
		}
		value.Add(item);
	}
}
