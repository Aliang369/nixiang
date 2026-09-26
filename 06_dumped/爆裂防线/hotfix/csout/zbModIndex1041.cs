public class zbModIndex1041 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1041 stModIndex2146 = new stModIndex1041();
		stModIndex2146.cdPreRatio = modRecord.clientEff[0];
		stModIndex2146.cdMaxRatio = modRecord.clientEff[1];
		stModIndex2146.skillType = modRecord.skillType[0];
		team.modIndex1041.Add(stModIndex2146);
	}
}
