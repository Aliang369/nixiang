public class zbModIndex1058 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.ExtraProperties[ETeamExtra.GemMod1058_1] = modRecord.clientEff[0];
		team.ExtraProperties[ETeamExtra.GemMod1058_2] = modRecord.clientEff[1];
	}
}
