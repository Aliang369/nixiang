public class zbModIndex1032 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1032 stModIndex2146 = new stModIndex1032();
		stModIndex2146.roll0to1 = modRecord.clientEff[0];
		team.modIndex1032.Add(stModIndex2146);
	}
}
