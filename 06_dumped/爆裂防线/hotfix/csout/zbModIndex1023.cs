public class zbModIndex1023 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.subSkillAddPenetration[(int)modRecord.clientEff[0]] += (int)modRecord.clientEff[1];
	}
}
