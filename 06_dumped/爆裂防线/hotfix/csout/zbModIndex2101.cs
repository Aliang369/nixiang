public class zbModIndex2101 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.ExtraProperties[ETeamExtra.GemMod2101] = modRecord.clientEff[0];
	}
}
