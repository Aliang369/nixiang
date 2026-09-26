public class zbModIndex1010 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stDesignSkillHitBuffToSubSkill item = new stDesignSkillHitBuffToSubSkill
		{
			srcSkillId = (int)modRecord.clientEff[0],
			srcBuffId = (int)modRecord.clientEff[1],
			subCount = (int)modRecord.clientEff[2],
			subSkillId = (int)modRecord.clientEff[3],
			skillType = modRecord.skillType[0]
		};
		team.designSkillHitBuffToSubSkill.Add(item);
	}
}
