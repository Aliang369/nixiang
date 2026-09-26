public class zbModIndex2011 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.ExtraProperties[ETeamExtra.GemMod2011_1] = modRecord.clientEff[0];
		team.ExtraProperties[ETeamExtra.GemMod2011_2] = modRecord.clientEff[1];
	}
}
