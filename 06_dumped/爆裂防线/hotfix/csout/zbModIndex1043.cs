public class zbModIndex1043 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1043 stModIndex2146 = new stModIndex1043();
		stModIndex2146.hpGreaterPresent = modRecord.clientEff[0];
		stModIndex2146.hpGreaterDamageAdd = modRecord.clientEff[1];
		team.modIndex1043.Add(stModIndex2146);
	}
}
