public class zbModIndex1031 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1031 stModIndex2146 = new stModIndex1031();
		stModIndex2146.extraDamageRatio = modRecord.clientEff[0];
		team.modIndex1031.Add(stModIndex2146);
	}
}
