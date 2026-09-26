public class zbModIndex1052 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1052 item = new stModIndex1052
		{
			extraRatio = modRecord.clientEff[0]
		};
		team.modIndex1052.Add(item);
	}
}
