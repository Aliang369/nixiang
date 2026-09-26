public class zbModIndex1053 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1053 item = new stModIndex1053
		{
			distance = modRecord.clientEff[0] / 100f,
			extraDamageRatio = modRecord.clientEff[1]
		};
		team.modIndex1053.Add(item);
	}
}
