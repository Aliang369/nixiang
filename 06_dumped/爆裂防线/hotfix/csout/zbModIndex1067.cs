public class zbModIndex1067 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1067 stModIndex2146 = new stModIndex1067();
		stModIndex2146.srcSkillId = (int)modRecord.clientEff[0];
		stModIndex2146.interval = modRecord.clientEff[1];
		stModIndex2146.subCount = (int)modRecord.clientEff[2];
		stModIndex2146.subSkillId = (int)modRecord.clientEff[3];
		stModIndex2146.skillType = modRecord.skillType[0];
		stModIndex2146.intervalKeep = stModIndex2146.interval;
		team.modIndex1067.Add(stModIndex2146);
	}
}
