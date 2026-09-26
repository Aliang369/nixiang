public class zbModIndex1022 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stMainIntervalToSubSkill item = new stMainIntervalToSubSkill
		{
			damageInterval = modRecord.clientEff[0],
			subCount = (int)modRecord.clientEff[1],
			subSkillId = (int)modRecord.clientEff[2],
			skillType = modRecord.skillType[0]
		};
		team.mainIntervalToSubSkill.Add(item);
	}
}
