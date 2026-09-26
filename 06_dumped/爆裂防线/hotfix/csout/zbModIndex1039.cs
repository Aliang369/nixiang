public class zbModIndex1039 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1039 stModIndex2146 = new stModIndex1039();
		stModIndex2146.extraDamageRatio = modRecord.clientEff[0];
		stModIndex2146.skillType = modRecord.skillType[0];
		team.modIndex1039.Add(stModIndex2146);
	}
}
