public class zbModIndex2114 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.ExtraProperties[ETeamExtra.GemMod2114] += modRecord.clientEff[0];
	}
}
