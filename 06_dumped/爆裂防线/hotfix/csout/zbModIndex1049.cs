public class zbModIndex1049 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1049 stModIndex2146 = new stModIndex1049();
		stModIndex2146.preHpPercent = modRecord.clientEff[0];
		stModIndex2146.subCount = (int)modRecord.clientEff[1];
		stModIndex2146.skillType = modRecord.skillType[0];
		stModIndex2146.lastLostHp = 0L;
		team.modIndex1049.Add(stModIndex2146);
	}
}
