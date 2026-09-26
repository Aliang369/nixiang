public class zbModIndex2061 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.modIndex2061.roll0to1 = modRecord.clientEff[0];
		team.modIndex2061.skillCount = (int)modRecord.clientEff[1];
	}
}
