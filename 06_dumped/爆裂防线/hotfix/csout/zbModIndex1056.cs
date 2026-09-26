public class zbModIndex1056 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.ExtraProperties[ETeamExtra.GemMod1056] += modRecord.clientEff[0];
	}
}
