public class zbModIndex1005 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stSubHitToSubSkill item = new stSubHitToSubSkill
		{
			srcSubSkillId = (int)modRecord.clientEff[0],
			subCount = (int)modRecord.clientEff[1],
			subSkillId = (int)modRecord.clientEff[2]
		};
		team.subHitToSubSkill.Add(item);
	}
}
