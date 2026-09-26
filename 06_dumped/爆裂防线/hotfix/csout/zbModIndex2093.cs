public class zbModIndex2093 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex2093 stModIndex2146 = new stModIndex2093();
		stModIndex2146.releaseNumber = (int)modRecord.clientEff[0];
		stModIndex2146.extraNumber = (int)modRecord.clientEff[1];
		stModIndex2146.skillType = modRecord.skillType[0];
		stModIndex2146.canAction = false;
		team.modIndex2093.Add(stModIndex2146);
	}
}
