public class zbModIndex2071 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.ExtraProperties[ETeamExtra.DianciMod2071] = modRecord.clientEff[0];
	}
}
