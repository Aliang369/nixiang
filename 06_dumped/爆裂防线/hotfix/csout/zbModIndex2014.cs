public class zbModIndex2014 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex2014 stModIndex2146 = new stModIndex2014();
		stModIndex2146.releaseNumber = (int)modRecord.clientEff[0];
		stModIndex2146.subCount = (int)modRecord.clientEff[1];
		stModIndex2146.subSkillId = (int)modRecord.clientEff[2];
		stModIndex2146.tickNumber = 0;
		team.modIndex2014.Add(stModIndex2146);
	}
}
