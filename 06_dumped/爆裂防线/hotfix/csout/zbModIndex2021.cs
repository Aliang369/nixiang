public class zbModIndex2021 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex2021 stModIndex2146 = new stModIndex2021();
		stModIndex2146.releaseNumber = (int)modRecord.clientEff[0];
		stModIndex2146.roll0to1 = modRecord.clientEff[1];
		stModIndex2146.extraNumber = (int)modRecord.clientEff[2];
		stModIndex2146.skillType = modRecord.skillType[0];
		team.modIndex2021.Add(stModIndex2146);
	}
}
