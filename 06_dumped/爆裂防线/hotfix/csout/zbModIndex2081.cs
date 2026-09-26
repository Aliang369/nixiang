public class zbModIndex2081 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.ExtraProperties[ETeamExtra.WurenjiMod2081] = modRecord.clientEff[0];
	}
}
