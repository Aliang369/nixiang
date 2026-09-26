public class zbModIndex1033 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1033 stModIndex2146 = new stModIndex1033();
		stModIndex2146.extraDamageRatio = modRecord.clientEff[0];
		team.modIndex1033.Add(stModIndex2146);
	}
}
