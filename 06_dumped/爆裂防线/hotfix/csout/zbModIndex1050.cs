public class zbModIndex1050 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.ExtraProperties[ETeamExtra.GemMod1050] += modRecord.clientEff[0];
	}
}
