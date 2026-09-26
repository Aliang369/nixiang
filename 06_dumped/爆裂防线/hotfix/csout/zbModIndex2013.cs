public class zbModIndex2013 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.ExtraProperties[ETeamExtra.GemMod2013] = modRecord.clientEff[0];
	}
}
