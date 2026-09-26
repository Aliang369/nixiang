public class zbModIndex1027 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stSkillEndToSubSkill item = new stSkillEndToSubSkill
		{
			srcSkillId = (int)modRecord.clientEff[0],
			subCount = (int)modRecord.clientEff[1],
			subSkillId = (int)modRecord.clientEff[2]
		};
		team.skillEndToSubSkill.Add(item);
	}
}
