public class zbModIndex1046 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.ExtraProperties[ETeamExtra.GemMod1046] += modRecord.clientEff[0];
	}
}
