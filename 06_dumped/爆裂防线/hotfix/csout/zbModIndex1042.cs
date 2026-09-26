public class zbModIndex1042 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1042 stModIndex2146 = new stModIndex1042();
		stModIndex2146.extraPreRatio = modRecord.clientEff[0];
		stModIndex2146.extraMaxRatio = modRecord.clientEff[1];
		stModIndex2146.skillType = modRecord.skillType[0];
		team.modIndex1042.Add(stModIndex2146);
	}
}
