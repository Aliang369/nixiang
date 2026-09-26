public class zbModIndex1054 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.ExtraProperties[ETeamExtra.GemMod1054] = modRecord.clientEff[0];
	}
}
