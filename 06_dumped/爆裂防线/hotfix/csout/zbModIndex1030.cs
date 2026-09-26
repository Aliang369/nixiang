public class zbModIndex1030 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1030 stModIndex2146 = new stModIndex1030();
		stModIndex2146.extraDamageRatio = modRecord.clientEff[0];
		stModIndex2146.limitDamageRatio = modRecord.clientEff[1];
		team.modIndex1030.Add(stModIndex2146);
	}
}
