public class zbModIndex2123 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.ExtraProperties[ETeamExtra.DianqiuMod2123] = modRecord.clientEff[0];
	}
}
