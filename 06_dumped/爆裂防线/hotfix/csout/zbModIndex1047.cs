public class zbModIndex1047 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.ExtraProperties[ETeamExtra.GemMod1047] += modRecord.clientEff[0];
	}
}
