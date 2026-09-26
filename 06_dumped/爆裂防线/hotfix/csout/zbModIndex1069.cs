public class zbModIndex1069 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1069 stModIndex2146 = new stModIndex1069();
		stModIndex2146.roll0to1 = modRecord.clientEff[0];
		stModIndex2146.damagePercent = modRecord.clientEff[1];
		stModIndex2146.skillType = modRecord.skillType[0];
		team.modIndex1069.Add(stModIndex2146);
	}
}
