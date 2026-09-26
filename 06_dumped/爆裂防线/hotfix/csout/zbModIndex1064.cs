public class zbModIndex1064 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1064 stModIndex2146 = new stModIndex1064();
		stModIndex2146.violentRateBase = modRecord.clientEff[0];
		team.modIndex1064.Add(stModIndex2146);
	}
}
