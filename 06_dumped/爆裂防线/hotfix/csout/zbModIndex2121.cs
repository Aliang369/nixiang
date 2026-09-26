public class zbModIndex2121 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.ExtraProperties[ETeamExtra.GemMod2121] = modRecord.clientEff[0];
	}
}
