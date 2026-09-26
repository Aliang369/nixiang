public class zbModIndex1004 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.subSkillAddDamage[(int)modRecord.clientEff[0]] *= 1f + modRecord.clientEff[1];
		team.subSkillAddDuration[(int)modRecord.clientEff[0]] += modRecord.clientEff[2];
		team.subSkillAddDamageRange[(int)modRecord.clientEff[0]] += modRecord.clientEff[3];
	}
}
