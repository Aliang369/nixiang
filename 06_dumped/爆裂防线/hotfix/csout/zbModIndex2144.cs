public class zbModIndex2144 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.modIndex2144.subBuffId = (int)modRecord.clientEff[0];
		team.modIndex2144.knockbackPrecent = modRecord.clientEff[1];
		team.modIndex2144.damagePrecent = modRecord.clientEff[2];
		team.modIndex2144.skillType = modRecord.skillType[0];
	}
}
