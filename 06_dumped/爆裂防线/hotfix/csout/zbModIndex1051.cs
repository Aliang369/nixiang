public class zbModIndex1051 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1051 item = new stModIndex1051
		{
			extraDamageRatio = modRecord.clientEff[0]
		};
		team.modIndex1051.Add(item);
	}
}
