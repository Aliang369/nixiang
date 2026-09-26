public class zbModIndex2143 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		if (modRecord.clientEff[0] > team.ExtraProperties[ETeamExtra.BingdanMod2143])
		{
			team.ExtraProperties[ETeamExtra.BingdanMod2143] = modRecord.clientEff[0];
		}
	}
}
