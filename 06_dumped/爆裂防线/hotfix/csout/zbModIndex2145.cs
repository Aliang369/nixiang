public class zbModIndex2145 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.modIndex2145.subBuffId = (int)modRecord.clientEff[0];
		team.modIndex2145.knockbackPrecent = modRecord.clientEff[1];
		team.modIndex2145.damagePrecent = modRecord.clientEff[2];
		team.modIndex2145.skillType = modRecord.skillType[0];
	}
}
