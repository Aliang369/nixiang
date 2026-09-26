public class zbModIndex1029 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1029 stModIndex2146 = new stModIndex1029();
		stModIndex2146.extraDamageRatio = modRecord.clientEff[0];
		stModIndex2146.limitDamageRatio = modRecord.clientEff[1];
		team.modIndex1029.Add(stModIndex2146);
	}
}
