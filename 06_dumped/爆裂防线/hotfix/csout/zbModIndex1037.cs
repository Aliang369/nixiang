public class zbModIndex1037 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1037 stModIndex2146 = new stModIndex1037();
		stModIndex2146.releaseNumber = (int)modRecord.clientEff[0];
		stModIndex2146.extraNumber = (int)modRecord.clientEff[1];
		stModIndex2146.subSkillId = (int)modRecord.clientEff[2];
		stModIndex2146.skillType = modRecord.skillType[0];
		team.modIndex1037.Add(stModIndex2146);
	}
}
