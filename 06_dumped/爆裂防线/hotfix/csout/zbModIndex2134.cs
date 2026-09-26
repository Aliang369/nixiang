public class zbModIndex2134 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex2134 stModIndex2146 = new stModIndex2134();
		stModIndex2146.releaseNumber = (int)modRecord.clientEff[0];
		stModIndex2146.extraNumber = (int)modRecord.clientEff[1];
		stModIndex2146.subSkillId = (int)modRecord.clientEff[2];
		stModIndex2146.skillType = modRecord.skillType[0];
		stModIndex2146.releaseNumberSum = stModIndex2146.releaseNumber;
		team.modIndex2134.Add(stModIndex2146);
	}
}
