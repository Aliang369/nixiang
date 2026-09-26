public class zbModIndex2012 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.ExtraProperties[ETeamExtra.GemMod2012_1] = modRecord.clientEff[0];
		team.ExtraProperties[ETeamExtra.GemMod2012_2] = modRecord.clientEff[1];
	}
}
