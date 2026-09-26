public class zbModIndex1057 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.ExtraProperties[ETeamExtra.GemMod1057_1] = modRecord.clientEff[0];
		team.ExtraProperties[ETeamExtra.GemMod1057_2] = modRecord.clientEff[1];
	}
}
