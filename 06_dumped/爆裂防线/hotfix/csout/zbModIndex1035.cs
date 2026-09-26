public class zbModIndex1035 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1035 stModIndex2146 = new stModIndex1035();
		stModIndex2146.nextFireTimeRatio = modRecord.clientEff[0];
		stModIndex2146.skillType = modRecord.skillType[0];
		team.modIndex1035.Add(stModIndex2146);
	}
}
