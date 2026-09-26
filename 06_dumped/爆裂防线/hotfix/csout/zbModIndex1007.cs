public class zbModIndex1007 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stMainSkillBuffExtraDamage item = new stMainSkillBuffExtraDamage
		{
			srcBuffId = (int)modRecord.clientEff[0],
			roll0to1 = modRecord.clientEff[1],
			limitDamageRatio = modRecord.clientEff[2],
			skillType = modRecord.skillType[0]
		};
		team.mainSkillBuffExtraDamage.Add(item);
	}
}
