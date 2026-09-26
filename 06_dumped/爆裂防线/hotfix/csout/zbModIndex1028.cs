public class zbModIndex1028 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stSubSkillEndToSubSkill item = new stSubSkillEndToSubSkill
		{
			srcSubSkillId = (int)modRecord.clientEff[0],
			subCount = (int)modRecord.clientEff[1],
			subSkillId = (int)modRecord.clientEff[2]
		};
		team.subSkillEndToSubSkill.Add(item);
	}
}
