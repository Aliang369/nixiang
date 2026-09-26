public class zbModIndex1076 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1076 stModIndex2146 = new stModIndex1076();
		stModIndex2146.srcSubSkillId = (int)modRecord.clientEff[0];
		stModIndex2146.roll0to1 = modRecord.clientEff[1];
		stModIndex2146.subBuffId = (int)modRecord.clientEff[2];
		stModIndex2146.extraTime = modRecord.clientEff[3];
		stModIndex2146.skillType = modRecord.skillType[0];
		team.modIndex1076.Add(stModIndex2146);
	}
}
