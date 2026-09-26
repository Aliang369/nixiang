public class zbModIndex1025 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1025 stModIndex2146 = new stModIndex1025();
		stModIndex2146.srcSkillId = (int)modRecord.clientEff[0];
		stModIndex2146.subCount = (int)modRecord.clientEff[1];
		stModIndex2146.subSkillId = (int)modRecord.clientEff[2];
		stModIndex2146.skillType = modRecord.skillType[0];
		team.modIndex1025.Add(stModIndex2146);
	}
}
