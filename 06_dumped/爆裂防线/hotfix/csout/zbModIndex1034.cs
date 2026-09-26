public class zbModIndex1034 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1034 stModIndex2146 = new stModIndex1034();
		stModIndex2146.roll0to1 = modRecord.clientEff[0];
		stModIndex2146.extraNumber = (int)modRecord.clientEff[1];
		stModIndex2146.skillType = modRecord.skillType[0];
		team.modIndex1034.Add(stModIndex2146);
	}
}
