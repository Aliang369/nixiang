public class zbModIndex1019 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stMainHitDefaultToSubSkill item = new stMainHitDefaultToSubSkill
		{
			subCount = (int)modRecord.clientEff[0],
			subSkillId = (int)modRecord.clientEff[1],
			skillType = modRecord.skillType[0]
		};
		team.mainHitDefaultToSubSkill.Add(item);
	}
}
