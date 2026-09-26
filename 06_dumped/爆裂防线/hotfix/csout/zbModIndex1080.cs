public class zbModIndex1080 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1080 stModIndex2146 = new stModIndex1080();
		stModIndex2146.killTotleCount = (int)modRecord.clientEff[0];
		stModIndex2146.subCount = (int)modRecord.clientEff[1];
		stModIndex2146.subSkillId = (int)modRecord.clientEff[2];
		stModIndex2146.skillType = (int)modRecord.clientEff[3];
		stModIndex2146.attrSkillType = (int)modRecord.clientEff[4];
		stModIndex2146.attrValue = modRecord.clientEff[5];
		stModIndex2146.curKillCount = 0;
		team.modIndex1080.Add(stModIndex2146);
	}
}
