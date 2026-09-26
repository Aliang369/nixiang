public class zbModIndex2124 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.ExtraProperties[ETeamExtra.GemMod2124] = modRecord.clientEff[0];
	}
}
